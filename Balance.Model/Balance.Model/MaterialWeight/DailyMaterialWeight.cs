﻿using Balance.Infrastructure.BasicDate;
using Balance.Infrastructure.Configuration;
using SqlServerDataAdapter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balance.Model.MaterialWeight
{
    public class DailyMaterialWeight
    {
        public static DataTable GetDailyMaterialWeightS()
        {
            SingleBasicData singleBasicData = SingleBasicData.Creat();
            SingleTimeService singleTimeserver = SingleTimeService.Creat();
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string m_SqlColumns = @"SELECT * FROM [{0}].[dbo].[HistoryDCSIncrement] WHERE vDate <> vDate";
            try
            {
                DataTable m_ColumnsTable = dataFactory.Query(string.Format(m_SqlColumns, singleBasicData.AmmeterName));
                if (m_ColumnsTable != null)
                {
                    string m_Sql = "";
                    string m_Columns = "";
                    for (int i = 1; i < m_ColumnsTable.Columns.Count; i++)
                    {
                        m_Columns = m_Columns + string.Format(", sum({0}) as {0}", m_ColumnsTable.Columns[i].ColumnName);
                    }
                    string m_SqlTemplate = @"SELECT '{3}' as Id {0} FROM [{1}].[dbo].[HistoryDCSIncrement] WHERE {2}";

                    if ("1=0" != singleTimeserver.PeakTimeCriterion)//峰期
                    {
                        if (m_Sql == "")
                        {
                            m_Sql = string.Format(m_SqlTemplate, m_Columns, singleBasicData.AmmeterName, singleTimeserver.PeakTimeCriterion, "Peak");
                        }
                        else
                        {
                            m_Sql = m_Sql + " union all " + string.Format(m_SqlTemplate, m_Columns, singleBasicData.AmmeterName, singleTimeserver.PeakTimeCriterion, "Peak");
                        }
                    }
                    if ("1=0" != singleTimeserver.MorePeakTimeCriterion)//尖峰期
                    {
                        if (m_Sql == "")
                        {
                            m_Sql = string.Format(m_SqlTemplate, m_Columns, singleBasicData.AmmeterName, singleTimeserver.MorePeakTimeCriterion, "MorePeak");
                        }
                        else
                        {
                            m_Sql = m_Sql + " union all " + string.Format(m_SqlTemplate, m_Columns, singleBasicData.AmmeterName, singleTimeserver.MorePeakTimeCriterion, "MorePeak");
                        }
                    }
                    if ("1=0" != singleTimeserver.ValleyTimeCriterion)//谷期
                    {
                        if (m_Sql == "")
                        {
                            m_Sql = string.Format(m_SqlTemplate, m_Columns, singleBasicData.AmmeterName, singleTimeserver.ValleyTimeCriterion, "Valley");
                        }
                        else
                        {
                            m_Sql = m_Sql + " union all " + string.Format(m_SqlTemplate, m_Columns, singleBasicData.AmmeterName, singleTimeserver.ValleyTimeCriterion, "Valley");
                        }
                    }
                    if ("1=0" != singleTimeserver.MoreValleyTimeCriterion)//深谷期
                    {
                        if (m_Sql == "")
                        {
                            m_Sql = string.Format(m_SqlTemplate, m_Columns, singleBasicData.AmmeterName, singleTimeserver.MoreValleyTimeCriterion, "MoreValley");
                        }
                        else
                        {
                            m_Sql = m_Sql + " union all " + string.Format(m_SqlTemplate, m_Columns, singleBasicData.AmmeterName, singleTimeserver.MoreValleyTimeCriterion, "MoreValley");
                        }
                    }
                    if ("1=0" != singleTimeserver.FlatTimeCriterion)//平期
                    {
                        if (m_Sql == "")
                        {
                            m_Sql = string.Format(m_SqlTemplate, m_Columns, singleBasicData.AmmeterName, singleTimeserver.FlatTimeCriterion, "Flat");
                        }
                        else
                        {
                            m_Sql = m_Sql + " union all " + string.Format(m_SqlTemplate, m_Columns, singleBasicData.AmmeterName, singleTimeserver.FlatTimeCriterion, "Flat");
                        }
                    }
                    if ("1=0" != singleTimeserver.FirstTimeCriterion)//甲班
                    {
                        if (m_Sql == "")
                        {
                            m_Sql = string.Format(m_SqlTemplate, m_Columns, singleBasicData.AmmeterName, singleTimeserver.FirstTimeCriterion, "First");
                        }
                        else
                        {
                            m_Sql = m_Sql + " union all " + string.Format(m_SqlTemplate, m_Columns, singleBasicData.AmmeterName, singleTimeserver.FirstTimeCriterion, "First");
                        }
                    }
                    if ("1=0" != singleTimeserver.SecondTimeCriterion)//乙班
                    {
                        if (m_Sql == "")
                        {
                            m_Sql = string.Format(m_SqlTemplate, m_Columns, singleBasicData.AmmeterName, singleTimeserver.SecondTimeCriterion, "Second");
                        }
                        else
                        {
                            m_Sql = m_Sql + " union all " + string.Format(m_SqlTemplate, m_Columns, singleBasicData.AmmeterName, singleTimeserver.SecondTimeCriterion, "Second");
                        }
                    }
                    if ("1=0" != singleTimeserver.ThirdTimeCriterion)//丙班
                    {
                        if (m_Sql == "")
                        {
                            m_Sql = string.Format(m_SqlTemplate, m_Columns, singleBasicData.AmmeterName, singleTimeserver.ThirdTimeCriterion, "Third");
                        }
                        else
                        {
                            m_Sql = m_Sql + " union all " + string.Format(m_SqlTemplate, m_Columns, singleBasicData.AmmeterName, singleTimeserver.ThirdTimeCriterion, "Third");
                        }
                    }
                    if (m_Sql != "")
                    {
                        DataTable MaterialWeightTable = dataFactory.Query(m_Sql);
                        return MaterialWeightTable;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

        }
        public static DataTable GetDailyMaterialWeight(DataTable myMaterialWeightS)
        {
            SingleBasicData singleBasicData = SingleBasicData.Creat();
            SingleTimeService singleTimeserver = SingleTimeService.Creat();
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            DataTable result = singleBasicData.BalanceTable.Clone();
            string sql = @"SELECT A.OrganizationID,B.VariableId,(A.Name+B.Name) AS Name,B.TagTableName,B.Formula
                            FROM tz_Material AS A,material_MaterialDetail AS B,system_Organization AS C, system_Organization AS D
                            WHERE A.KeyID=B.KeyID AND
                            A.OrganizationID=C.OrganizationID AND
                            A.Enable='True' AND
                            A.State=0 AND
							D.OrganizationID = @organizationId AND
                            C.LevelCode like D.LevelCode + '%'";
            SqlParameter parameter = new SqlParameter("organizationId", singleBasicData.OrganizationId);
            //需要保存的balance_Energy的质量信息
            DataTable variableInfo = dataFactory.Query(sql, parameter);
            //StringBuilder sqlBuilder = new StringBuilder();
            //sqlBuilder.Append("SELECT ");
            //string mySql = "SELECT SUM({0}) AS Value FROM [{1}].[dbo].[HistoryDCSIncrement] WHERE {2} ";
            foreach (DataRow dr in variableInfo.Rows)
            {
                DataRow row = result.NewRow();
                row["VariableItemId"] = Guid.NewGuid().ToString();//ID
                row["VariableId"] = dr["VariableId"].ToString().Trim();
                row["VariableName"] = dr["Name"].ToString().Trim();
                row["PublicVariableId"] = row["KeyId"] = singleBasicData.KeyId;
                row["OrganizationID"] = dr["OrganizationID"].ToString().Trim();
                row["ValueType"] = "MaterialWeight";
                string[] arrayFields = { "TotalPeakValleyFlat", "MorePeak", "Peak", "Valley", "MoreValley", "Flat", "First", "Second", "Third" };
                foreach (string item in arrayFields)
                {
                    row[item] = 0;
                }
                result.Rows.Add(row);
                /////////////////////////用于构成公式计算列////////////////
                string m_ColumnName = dr["Formula"].ToString().Trim();
                bool m_IsMultiColumn = false;
                if (!myMaterialWeightS.Columns.Contains(m_ColumnName))
                {
                    DataColumn m_ClAmount = new DataColumn(m_ColumnName, typeof(decimal));
                    if (m_ColumnName != "0")
                    {
                        m_ClAmount.Expression = m_ColumnName;
                    }
                    myMaterialWeightS.Columns.Add(m_ClAmount);
                    m_IsMultiColumn = true;
                }
                /////////////////////////
                //*****************
                try
                {
                    if ("1=0" != singleTimeserver.PeakTimeCriterion)//峰期
                    {
                        Object m_SumObject = myMaterialWeightS.Compute(string.Format("sum([{0}])", m_ColumnName), "Id = 'Peak'");
                        row["Peak"] = m_SumObject != null ? MyToDecimal(m_SumObject) : 0;
                        //DataTable peak = dataFactory.Query(string.Format(mySql, dr["Formula"].ToString().Trim(),singleBasicData.AmmeterName,singleTimeserver.PeakTimeCriterion));
                        //row["Peak"] = peak.Rows.Count == 0 ? 0 : MyToDecimal(peak.Rows[0]["Value"]);
                    }
                    else
                    {
                        row["Peak"] = 0;
                    }
                    if ("1=0" != singleTimeserver.MorePeakTimeCriterion)//尖峰期
                    {
                        Object m_SumObject = myMaterialWeightS.Compute(string.Format("sum([{0}])", m_ColumnName), "Id = 'MorePeak'");
                        row["MorePeak"] = m_SumObject != null ? MyToDecimal(m_SumObject) : 0;
                        //DataTable peak = dataFactory.Query(string.Format(mySql, dr["Formula"].ToString().Trim(), singleBasicData.AmmeterName, singleTimeserver.MorePeakTimeCriterion));
                        //row["MorePeak"] = peak.Rows.Count == 0 ? 0 : MyToDecimal(peak.Rows[0]["Value"]);
                    }
                    else
                    {
                        row["MorePeak"] = 0;
                    }
                    if ("1=0" != singleTimeserver.ValleyTimeCriterion)//谷期
                    {
                        Object m_SumObject = myMaterialWeightS.Compute(string.Format("sum([{0}])", m_ColumnName), "Id = 'Valley'");
                        row["Valley"] = m_SumObject != null ? MyToDecimal(m_SumObject) : 0;
                        //DataTable peak = dataFactory.Query(string.Format(mySql, dr["Formula"].ToString().Trim(), singleBasicData.AmmeterName, singleTimeserver.ValleyTimeCriterion));
                        //row["Valley"] = peak.Rows.Count == 0 ? 0 : MyToDecimal(peak.Rows[0]["Value"]);
                    }
                    else
                    {
                        row["Valley"] = 0;
                    }
                    if ("1=0" != singleTimeserver.MoreValleyTimeCriterion)//深谷期
                    {
                        Object m_SumObject = myMaterialWeightS.Compute(string.Format("sum([{0}])", m_ColumnName), "Id = 'MoreValley'");
                        row["MoreValley"] = m_SumObject != null ? MyToDecimal(m_SumObject) : 0;
                        //DataTable peak = dataFactory.Query(string.Format(mySql, dr["Formula"].ToString().Trim(), singleBasicData.AmmeterName, singleTimeserver.MoreValleyTimeCriterion));
                        //row["MoreValley"] = peak.Rows.Count == 0 ? 0 : MyToDecimal(peak.Rows[0]["Value"]);
                    }
                    else
                    {
                        row["MoreValley"] = 0;
                    }
                    if ("1=0" != singleTimeserver.FlatTimeCriterion)//平期
                    {
                        Object m_SumObject = myMaterialWeightS.Compute(string.Format("sum([{0}])", m_ColumnName), "Id = 'Flat'");
                        row["Flat"] = m_SumObject != null ? MyToDecimal(m_SumObject) : 0;
                        //DataTable peak = dataFactory.Query(string.Format(mySql, dr["Formula"].ToString().Trim(), singleBasicData.AmmeterName, singleTimeserver.FlatTimeCriterion));
                        //row["Flat"] = peak.Rows.Count == 0 ? 0 : MyToDecimal(peak.Rows[0]["Value"]);
                    }
                    else
                    {
                        row["Flat"] = 0;
                    }
                    if ("1=0" != singleTimeserver.FirstTimeCriterion)//甲班
                    {
                        Object m_SumObject = myMaterialWeightS.Compute(string.Format("sum([{0}])", m_ColumnName), "Id = 'First'");
                        row["First"] = m_SumObject != null ? MyToDecimal(m_SumObject) : 0;
                        //DataTable peak = dataFactory.Query(string.Format(mySql, dr["Formula"].ToString().Trim(), singleBasicData.AmmeterName, singleTimeserver.FirstTimeCriterion));
                        //row["First"] = peak.Rows.Count == 0 ? 0 : MyToDecimal(peak.Rows[0]["Value"]);
                    }
                    else
                    {
                        row["First"] = 0;
                    }
                    if ("1=0" != singleTimeserver.SecondTimeCriterion)//乙班
                    {
                        Object m_SumObject = myMaterialWeightS.Compute(string.Format("sum([{0}])", m_ColumnName), "Id = 'Second'");
                        row["Second"] = m_SumObject != null ? MyToDecimal(m_SumObject) : 0;
                        //DataTable peak = dataFactory.Query(string.Format(mySql, dr["Formula"].ToString().Trim(), singleBasicData.AmmeterName, singleTimeserver.SecondTimeCriterion));
                        //row["Second"] = peak.Rows.Count == 0 ? 0 : MyToDecimal(peak.Rows[0]["Value"]);
                    }
                    else
                    {
                        row["Second"] = 0;
                    }
                    if ("1=0" != singleTimeserver.ThirdTimeCriterion)//丙班
                    {
                        Object m_SumObject = myMaterialWeightS.Compute(string.Format("sum([{0}])", m_ColumnName), "Id = 'Third'");
                        row["Third"] = m_SumObject != null ? MyToDecimal(m_SumObject) : 0;
                        //DataTable peak = dataFactory.Query(string.Format(mySql, dr["Formula"].ToString().Trim(), singleBasicData.AmmeterName, singleTimeserver.ThirdTimeCriterion));
                        //row["Third"] = peak.Rows.Count == 0 ? 0 : MyToDecimal(peak.Rows[0]["Value"]);
                    }
                    else
                    {
                        row["Third"] = 0;
                    }
                    ////////////////////////////////////
                    //**************
                    //string[] arrayFields = { "TotalPeakValleyFlat", "MorePeak", "Peak", "Valley", "MoreValley", "Flat", "First", "Second", "Third" };
                    foreach (string field in arrayFields)
                    {
                        if (field == "TotalPeakValleyFlat")
                        {
                            row[field] = row[field + "B"] = MyToDecimal(row["First"]) + MyToDecimal(row["Second"]) + MyToDecimal(row["Third"]);
                        }
                        else
                        {
                            row[field + "B"] = row[field];
                        }

                    }
                    //************
                }
                catch
                {

                }
                /////////////如果是运算列,则删除该列
                if (m_IsMultiColumn == true)
                {
                    myMaterialWeightS.Columns.Remove(m_ColumnName);
                }
            }
            //将今天的盘库量写到表中
            BalanceMartieralsClass.ProcessMartieralsClass(result);
            return result;
        }

        private static decimal MyToDecimal(object obj)
        {
            if (obj is DBNull)
            {
                obj = 0;
                return Convert.ToDecimal(obj);
            }
            else
            {
                return Convert.ToDecimal(obj);
            }
        }
        public static DataTable GetMaterialWeightSV(DataTable myMaterialWeightS, string myKeyId, string myOrganizationId)
        {
            DataTable m_MaterialWeightSVTable = new DataTable();
            return m_MaterialWeightSVTable;
        }


        //////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// /////////////////////
        /// </summary>
        /// <param name="myMaterialWeightS"></param>
        /// <param name="my"></param>
        /// <returns></returns>
        public static DataTable GetDailyEquipmentOutput(DataTable myMaterialWeightS)
        {
            SingleBasicData singleBasicData = SingleBasicData.Creat();
            SingleTimeService singleTimeserver = SingleTimeService.Creat();
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            DataTable result = singleBasicData.BalanceProductionTable.Clone();
            string sql = @"SELECT A.OrganizationID, A.EquipmentId AS EquipmentId, A.EquipmentName AS EquipmentName, A.OutputFormula AS Formula
                            FROM equipment_EquipmentDetail AS A,system_Organization AS C, system_Organization AS D
                            WHERE A.OrganizationID=C.OrganizationID AND
							D.OrganizationID = @organizationId AND
                            C.LevelCode like D.LevelCode + '%'";
            SqlParameter parameter = new SqlParameter("organizationId", singleBasicData.OrganizationId);
            //需要保存的balance_Energy的质量信息
            DataTable variableInfo = dataFactory.Query(sql, parameter);
            //StringBuilder sqlBuilder = new StringBuilder();
            //sqlBuilder.Append("SELECT ");
            //string mySql = "SELECT SUM({0}) AS Value FROM [{1}].[dbo].[HistoryDCSIncrement] WHERE {2} ";
            foreach (DataRow dr in variableInfo.Rows)
            {
                DataRow row = result.NewRow();
                row["VariableItemId"] = Guid.NewGuid().ToString();//ID
                row["VariableId"] = dr["EquipmentId"].ToString().Trim();
                row["VariableName"] = dr["EquipmentName"].ToString().Trim();
                row["PublicVariableId"] = row["KeyId"] = singleBasicData.KeyId;
                row["OrganizationID"] = dr["OrganizationID"].ToString().Trim();
                row["VariableType"] = "EquipmentOutput";
                row["ValueType"] = "MaterialWeight";
                string[] arrayFields = { "TotalPeakValleyFlat", "MorePeak", "Peak", "Valley", "MoreValley", "Flat", "First", "Second", "Third" };
                foreach (string item in arrayFields)
                {
                    row[item] = 0;
                }
                result.Rows.Add(row);
                /////////////////////////用于构成公式计算列////////////////
                string m_ColumnName = dr["Formula"].ToString().Trim();
                bool m_IsMultiColumn = false;
                if (!myMaterialWeightS.Columns.Contains(m_ColumnName))
                {
                    DataColumn m_ClAmount = new DataColumn(m_ColumnName, typeof(decimal));
                    if (m_ColumnName != "0")
                    {
                        m_ClAmount.Expression = m_ColumnName;
                    }
                    myMaterialWeightS.Columns.Add(m_ClAmount);
                    m_IsMultiColumn = true;
                }
                /////////////////////////
                //*****************
                try
                {
                    if ("1=0" != singleTimeserver.PeakTimeCriterion)//峰期
                    {
                        Object m_SumObject = myMaterialWeightS.Compute(string.Format("sum([{0}])", m_ColumnName), "Id = 'Peak'");
                        row["Peak"] = m_SumObject != null ? MyToDecimal(m_SumObject) : 0;
                        //DataTable peak = dataFactory.Query(string.Format(mySql, dr["Formula"].ToString().Trim(),singleBasicData.AmmeterName,singleTimeserver.PeakTimeCriterion));
                        //row["Peak"] = peak.Rows.Count == 0 ? 0 : MyToDecimal(peak.Rows[0]["Value"]);
                    }
                    else
                    {
                        row["Peak"] = 0;
                    }
                    if ("1=0" != singleTimeserver.MorePeakTimeCriterion)//尖峰期
                    {
                        Object m_SumObject = myMaterialWeightS.Compute(string.Format("sum([{0}])", m_ColumnName), "Id = 'MorePeak'");
                        row["MorePeak"] = m_SumObject != null ? MyToDecimal(m_SumObject) : 0;
                        //DataTable peak = dataFactory.Query(string.Format(mySql, dr["Formula"].ToString().Trim(), singleBasicData.AmmeterName, singleTimeserver.MorePeakTimeCriterion));
                        //row["MorePeak"] = peak.Rows.Count == 0 ? 0 : MyToDecimal(peak.Rows[0]["Value"]);
                    }
                    else
                    {
                        row["MorePeak"] = 0;
                    }
                    if ("1=0" != singleTimeserver.ValleyTimeCriterion)//谷期
                    {
                        Object m_SumObject = myMaterialWeightS.Compute(string.Format("sum([{0}])", m_ColumnName), "Id = 'Valley'");
                        row["Valley"] = m_SumObject != null ? MyToDecimal(m_SumObject) : 0;
                        //DataTable peak = dataFactory.Query(string.Format(mySql, dr["Formula"].ToString().Trim(), singleBasicData.AmmeterName, singleTimeserver.ValleyTimeCriterion));
                        //row["Valley"] = peak.Rows.Count == 0 ? 0 : MyToDecimal(peak.Rows[0]["Value"]);
                    }
                    else
                    {
                        row["Valley"] = 0;
                    }
                    if ("1=0" != singleTimeserver.MoreValleyTimeCriterion)//深谷期
                    {
                        Object m_SumObject = myMaterialWeightS.Compute(string.Format("sum([{0}])", m_ColumnName), "Id = 'MoreValley'");
                        row["MoreValley"] = m_SumObject != null ? MyToDecimal(m_SumObject) : 0;
                        //DataTable peak = dataFactory.Query(string.Format(mySql, dr["Formula"].ToString().Trim(), singleBasicData.AmmeterName, singleTimeserver.MoreValleyTimeCriterion));
                        //row["MoreValley"] = peak.Rows.Count == 0 ? 0 : MyToDecimal(peak.Rows[0]["Value"]);
                    }
                    else
                    {
                        row["MoreValley"] = 0;
                    }
                    if ("1=0" != singleTimeserver.FlatTimeCriterion)//平期
                    {
                        Object m_SumObject = myMaterialWeightS.Compute(string.Format("sum([{0}])", m_ColumnName), "Id = 'Flat'");
                        row["Flat"] = m_SumObject != null ? MyToDecimal(m_SumObject) : 0;
                        //DataTable peak = dataFactory.Query(string.Format(mySql, dr["Formula"].ToString().Trim(), singleBasicData.AmmeterName, singleTimeserver.FlatTimeCriterion));
                        //row["Flat"] = peak.Rows.Count == 0 ? 0 : MyToDecimal(peak.Rows[0]["Value"]);
                    }
                    else
                    {
                        row["Flat"] = 0;
                    }
                    if ("1=0" != singleTimeserver.FirstTimeCriterion)//甲班
                    {
                        Object m_SumObject = myMaterialWeightS.Compute(string.Format("sum([{0}])", m_ColumnName), "Id = 'First'");
                        row["First"] = m_SumObject != null ? MyToDecimal(m_SumObject) : 0;
                        //DataTable peak = dataFactory.Query(string.Format(mySql, dr["Formula"].ToString().Trim(), singleBasicData.AmmeterName, singleTimeserver.FirstTimeCriterion));
                        //row["First"] = peak.Rows.Count == 0 ? 0 : MyToDecimal(peak.Rows[0]["Value"]);
                    }
                    else
                    {
                        row["First"] = 0;
                    }
                    if ("1=0" != singleTimeserver.SecondTimeCriterion)//乙班
                    {
                        Object m_SumObject = myMaterialWeightS.Compute(string.Format("sum([{0}])", m_ColumnName), "Id = 'Second'");
                        row["Second"] = m_SumObject != null ? MyToDecimal(m_SumObject) : 0;
                        //DataTable peak = dataFactory.Query(string.Format(mySql, dr["Formula"].ToString().Trim(), singleBasicData.AmmeterName, singleTimeserver.SecondTimeCriterion));
                        //row["Second"] = peak.Rows.Count == 0 ? 0 : MyToDecimal(peak.Rows[0]["Value"]);
                    }
                    else
                    {
                        row["Second"] = 0;
                    }
                    if ("1=0" != singleTimeserver.ThirdTimeCriterion)//丙班
                    {
                        Object m_SumObject = myMaterialWeightS.Compute(string.Format("sum([{0}])", m_ColumnName), "Id = 'Third'");
                        row["Third"] = m_SumObject != null ? MyToDecimal(m_SumObject) : 0;
                        //DataTable peak = dataFactory.Query(string.Format(mySql, dr["Formula"].ToString().Trim(), singleBasicData.AmmeterName, singleTimeserver.ThirdTimeCriterion));
                        //row["Third"] = peak.Rows.Count == 0 ? 0 : MyToDecimal(peak.Rows[0]["Value"]);
                    }
                    else
                    {
                        row["Third"] = 0;
                    }
                    ////////////////////////////////////
                    //**************
                    //string[] arrayFields = { "TotalPeakValleyFlat", "MorePeak", "Peak", "Valley", "MoreValley", "Flat", "First", "Second", "Third" };
                    foreach (string field in arrayFields)
                    {
                        if (field == "TotalPeakValleyFlat")
                        {
                            row[field] = row[field + "B"] = MyToDecimal(row["First"]) + MyToDecimal(row["Second"]) + MyToDecimal(row["Third"]);
                        }
                        else
                        {
                            row[field + "B"] = row[field];
                        }

                    }
                    //************
                }
                catch
                {

                }
                /////////////如果是运算列,则删除该列
                if (m_IsMultiColumn == true)
                {
                    myMaterialWeightS.Columns.Remove(m_ColumnName);
                }
            }
            return result;
        }


        public static DataTable GetMaterialWeightProduction(DataTable myMaterialWeightS, string myOrganizationId, string myKeyId)
        {
            DataTable m_MaterialWeightDataTable = new DataTable();
            string[] m_ColumnsTemp = new[]{"TotalPeakValleyFlat","MorePeak","Peak","Valley","MoreValley","Flat","First","Second","Third",
                "TotalPeakValleyFlatB","MorePeakB","PeakB","ValleyB","MoreValleyB","FlatB","FirstB","SecondB","ThirdB"};
            m_MaterialWeightDataTable.Columns.Add("VariableItemId", typeof(System.Guid));
            m_MaterialWeightDataTable.Columns.Add("VariableId", typeof(string));
            m_MaterialWeightDataTable.Columns.Add("VariableName", typeof(string));
            m_MaterialWeightDataTable.Columns.Add("PublicVariableId", typeof(string));
            m_MaterialWeightDataTable.Columns.Add("KeyId", typeof(System.Guid));
            m_MaterialWeightDataTable.Columns.Add("OrganizationID", typeof(string));
            m_MaterialWeightDataTable.Columns.Add("VariableType", typeof(string));
            m_MaterialWeightDataTable.Columns.Add("ValueType", typeof(string));
            for (int i = 0; i < m_ColumnsTemp.Length; i++)
            {
                m_MaterialWeightDataTable.Columns.Add(m_ColumnsTemp[i], typeof(decimal));
            }
            if (myMaterialWeightS != null)
            {
                for (int i = 1; i < myMaterialWeightS.Columns.Count; i++)
                {
                    DataRow m_NewDataRow = m_MaterialWeightDataTable.NewRow();
                    for (int j = 0; j < m_ColumnsTemp.Length; j++)
                    {
                        m_NewDataRow[m_ColumnsTemp[j]] = 0.0m;
                    }
                    m_NewDataRow["VariableItemId"] = Guid.NewGuid().ToString();
                    m_NewDataRow["VariableId"] = myMaterialWeightS.Columns[i].ColumnName;
                    m_NewDataRow["VariableName"] = myMaterialWeightS.Columns[i].ColumnName;
                    m_NewDataRow["PublicVariableId"] = DBNull.Value;
                    m_NewDataRow["KeyId"] = myKeyId;
                    m_NewDataRow["OrganizationID"] = myOrganizationId;
                    m_NewDataRow["VariableType"] = "DirectTag";
                    m_NewDataRow["ValueType"] = "Increment";

                    for (int j = 0; j < myMaterialWeightS.Rows.Count; j++)
                    {
                        m_NewDataRow[myMaterialWeightS.Rows[j][0].ToString()] = myMaterialWeightS.Rows[j][i] == DBNull.Value ? 0.0m : (decimal)myMaterialWeightS.Rows[j][i];
                        m_NewDataRow[myMaterialWeightS.Rows[j][0].ToString() + "B"] = myMaterialWeightS.Rows[j][i] == DBNull.Value ? 0.0m : (decimal)myMaterialWeightS.Rows[j][i];
                    }
                    decimal m_First = m_NewDataRow["First"] == DBNull.Value ? 0.0m : (decimal)m_NewDataRow["First"];
                    decimal m_Second = m_NewDataRow["Second"] == DBNull.Value ? 0.0m : (decimal)m_NewDataRow["Second"];
                    decimal m_Third = m_NewDataRow["Third"] == DBNull.Value ? 0.0m : (decimal)m_NewDataRow["Third"];
                    m_NewDataRow["TotalPeakValleyFlat"] = m_First + m_Second + m_Third;
                    m_NewDataRow["TotalPeakValleyFlatB"] = m_First + m_Second + m_Third;
                    m_MaterialWeightDataTable.Rows.Add(m_NewDataRow);
                }
            }
            return m_MaterialWeightDataTable;
        }
        
    }
}
