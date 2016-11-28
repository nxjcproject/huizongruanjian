using Balance.Infrastructure.BasicDate;
using Balance.Infrastructure.Configuration;
using Balance.Model.ElectricityQuantity;
using Balance.Model.MaterialChange;
using Balance.Model.MaterialWeight;
using Balance.Model.TzBalance;
using SqlServerDataAdapter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balance.Model
{
    public class BalanceService
    {

        private static string connectionString = ConnectionStringFactory.NXJCConnectionString;
        private static ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
        public static void SetBalance(DateTime date)
        {
            string[] factorys = ConfigService.GetConfig("FactoryID").Split(',');
            foreach (string factory in factorys)
            {
                //检查四天之前的数据是否保存，没有则保存
                for (DateTime cursorDate = date.AddDays(-4); cursorDate <= date; cursorDate = cursorDate.AddDays(1))
                {
                    string mySql = @"select A.TimeStamp,A.OrganizationID
                                from tz_Balance A 
                                where A.StaticsCycle='day' and A.OrganizationID=@organizationId and A.TimeStamp=@checkDate";
                    SqlParameter[] parameters = { new SqlParameter("organizationId", factory),
                                        new SqlParameter("checkDate",cursorDate.ToString("yyyy-MM-dd"))};
                    DataTable checkTable = dataFactory.Query(mySql, parameters);
                    if (checkTable.Rows.Count == 0)
                    {
                        SaveData(factory, cursorDate);
                    }
                }
            }
        }
        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="organizationId">分厂组织机构ID</param>
        /// <param name="saveDate">保存数据的日期</param>
        private static void SaveData(string organizationId, DateTime saveDate)
        {
            SingleBasicData singleBasicData = SingleBasicData.Creat();
            singleBasicData.Init(organizationId, saveDate.ToString("yyyy-MM-dd"));
            SingleTimeService singleTimeService = SingleTimeService.Creat();
            //每天都重新初始化
            singleTimeService.Init(dataFactory);
            DataTable tzBalance = TzBalanceService.GetDailyTzBalance();
            DataTable electricity = DailyElectricityQuantityService.GetElectricQuantity();
            DataTable m_MaterialWeightS = DailyMaterialWeight.GetDailyMaterialWeightS();
            DataTable m_MaterialWeightProduction = DailyMaterialWeight.GetMaterialWeightProduction(m_MaterialWeightS, organizationId, singleBasicData.KeyId);
            DataTable m_EquipmentOutput = DailyMaterialWeight.GetDailyEquipmentOutput(m_MaterialWeightS);
            DataTable materialWeight = DailyMaterialWeight.GetDailyMaterialWeight(m_MaterialWeightS);
            DataTable m_m_MaterialWeightSV = DailyMaterialWeight.GetMaterialWeightSV(m_MaterialWeightS, singleBasicData.KeyId, singleBasicData.OrganizationId);    //横表变纵表
            //将电量产量消耗量合成一表
            electricity.Merge(materialWeight);
            m_MaterialWeightProduction.Merge(m_EquipmentOutput); 

            string sql = @"SELECT A.VariableId,B.OrganizationID,(B.Name+A.VariableName) AS Name,A.ValueType,A.ValueFormula
                                    FROM balance_Energy_Template AS A,system_Organization AS B
                                    WHERE 
                                    A.ProductionLineType=B.Type 
                                    AND (A.ValueType='ElectricityConsumption'
                                    OR A.ValueType='CoalConsumption')
                                    AND A.Enabled='True'
                                    AND B.OrganizationID like '{0}%'";
            DataTable template = dataFactory.Query(string.Format(sql, singleBasicData.OrganizationId));
            string[] columns ={"TotalPeakValleyFlat", "MorePeak", "Peak", "Valley", "MoreValley", "Flat", "First", "Second", "Third", "TotalPeakValleyFlatB", "MorePeakB", 
                "PeakB", "ValleyB", "MoreValleyB", "FlatB", "FirstB", "SecondB", "ThirdB"};
            DataTable consumptionTemp = EnergyConsumption.EnergyConsumptionCalculate.CalculateByOrganizationId(electricity, template, "ValueFormula", columns);
            DataTable consumption = singleBasicData.BalanceTable.Clone();
            foreach (DataRow dr in consumptionTemp.Rows)
            {
                DataRow row = consumption.NewRow();
                row["VariableItemId"] = Guid.NewGuid().ToString();
                foreach (DataColumn item in consumptionTemp.Columns)
                {
                    string name = item.ColumnName;
                    if (name == "ValueFormula")
                        continue;
                    if (name == "Name")
                        row["VariableName"] = dr[name];
                    else
                        row[name] = dr[name];
                    row["PublicVariableId"] = row["KeyId"] = singleBasicData.KeyId;
                }
                consumption.Rows.Add(row);
            }

            electricity.Merge(consumption);
            //获取水泥产量
            DataTable cementTable = DailyMaterialChangeSummation.GetMaterialChange();
            electricity.Merge(cementTable);
            //将数据保存到tz_balance和balance_Energy
            using (SqlConnection conn = new SqlConnection(ConnectionStringFactory.NXJCConnectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.CheckConstraints, transaction))
                    {
                        bulkCopy.BatchSize = 10;
                        bulkCopy.BulkCopyTimeout = 60;
                        try
                        {
                            bulkCopy.DestinationTableName = "tz_Balance";
                            bulkCopy.WriteToServer(tzBalance);

                            bulkCopy.DestinationTableName = "balance_Energy";
                            bulkCopy.WriteToServer(electricity);

                            bulkCopy.DestinationTableName = "balance_Production";
                            bulkCopy.WriteToServer(m_MaterialWeightProduction);
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            StreamWriter sw = File.AppendText(singleBasicData.Path);
                            sw.WriteLine("Error:" + DateTime.Now.ToString() + "保存数据失败！");
                            sw.Flush();
                            sw.Close();
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 根据KeyId和日期保存数据
        /// </summary>
        /// <param name="organizationId">[分厂]组织机构ID</param>
        /// <param name="keyId">引领表KeyId</param>
        /// <param name="saveDate">保存日期</param>
        public static void SaveDayBalanceDetailData(string myFactoryOrganizationId, DateTime saveDate)
        {
            SingleBasicData singleBasicData = SingleBasicData.Creat();
            singleBasicData.Init(myFactoryOrganizationId, saveDate.ToString("yyyy-MM-dd"));
            SingleTimeService singleTimeService = SingleTimeService.Creat();
            //每天都重新初始化
            singleTimeService.Init(dataFactory);
            DataTable tzBalance = TzBalanceService.GetDailyTzBalance();
            DataTable electricity = DailyElectricityQuantityService.GetElectricQuantity();
            DataTable m_MaterialWeightS = DailyMaterialWeight.GetDailyMaterialWeightS();
            DataTable m_MaterialWeightProduction = DailyMaterialWeight.GetMaterialWeightProduction(m_MaterialWeightS, myFactoryOrganizationId, singleBasicData.KeyId);
            DataTable m_EquipmentOutput = DailyMaterialWeight.GetDailyEquipmentOutput(m_MaterialWeightS);
            DataTable materialWeight = DailyMaterialWeight.GetDailyMaterialWeight(m_MaterialWeightS);
            DataTable m_m_MaterialWeightSV = DailyMaterialWeight.GetMaterialWeightSV(m_MaterialWeightS, singleBasicData.KeyId, singleBasicData.OrganizationId);    //横表变纵表
            //将电量产量消耗量合成一表
            electricity.Merge(materialWeight);
            m_MaterialWeightProduction.Merge(m_EquipmentOutput); 

            string sql = @"SELECT A.VariableId,B.OrganizationID,(B.Name+A.VariableName) AS Name,A.ValueType,A.ValueFormula
                                    FROM balance_Energy_Template AS A,system_Organization AS B
                                    WHERE 
                                    A.ProductionLineType=B.Type 
                                    AND (A.ValueType='ElectricityConsumption'
                                    OR A.ValueType='CoalConsumption')
                                    AND A.Enabled='True'
                                    AND B.OrganizationID like '{0}%'";
            DataTable template = dataFactory.Query(string.Format(sql, singleBasicData.OrganizationId));
            string[] columns ={"TotalPeakValleyFlat", "MorePeak", "Peak", "Valley", "MoreValley", "Flat", "First", "Second", "Third", "TotalPeakValleyFlatB", "MorePeakB", 
                "PeakB", "ValleyB", "MoreValleyB", "FlatB", "FirstB", "SecondB", "ThirdB"};
            DataTable consumptionTemp = EnergyConsumption.EnergyConsumptionCalculate.CalculateByOrganizationId(electricity, template, "ValueFormula", columns);
            DataTable consumption = singleBasicData.BalanceTable.Clone();
            foreach (DataRow dr in consumptionTemp.Rows)
            {
                DataRow row = consumption.NewRow();
                row["VariableItemId"] = Guid.NewGuid().ToString();
                foreach (DataColumn item in consumptionTemp.Columns)
                {
                    string name = item.ColumnName;
                    if (name == "ValueFormula")
                        continue;
                    if (name == "Name")
                        row["VariableName"] = dr[name];
                    else
                        row[name] = dr[name];
                    row["PublicVariableId"] = row["KeyId"] = singleBasicData.KeyId;
                }
                consumption.Rows.Add(row);
            }

            electricity.Merge(consumption);
            //获取水泥产量
            DataTable cementTable = DailyMaterialChangeSummation.GetMaterialChange();
            electricity.Merge(cementTable);
            //将数据保存到tz_balance和balance_Energy
            
            using (SqlConnection conn = new SqlConnection(ConnectionStringFactory.NXJCConnectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.CheckConstraints, transaction))
                    {
                        bulkCopy.BatchSize = 10;
                        bulkCopy.BulkCopyTimeout = 60;
                        try
                        {
                            bulkCopy.DestinationTableName = "tz_Balance";
                            bulkCopy.WriteToServer(tzBalance);

                            bulkCopy.DestinationTableName = "balance_Energy";
                            bulkCopy.WriteToServer(electricity);

                            bulkCopy.DestinationTableName = "balance_Production";
                            bulkCopy.WriteToServer(m_MaterialWeightProduction);
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }
        }

    }
}
