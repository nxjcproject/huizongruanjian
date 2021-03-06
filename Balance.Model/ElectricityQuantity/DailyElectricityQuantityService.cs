﻿using Balance.Infrastructure.BasicDate;
using Balance.Infrastructure.Configuration;
using Balance.Model.PublicMethod;
using SqlServerDataAdapter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balance.Model.ElectricityQuantity
{
    public class DailyElectricityQuantityService
    {
        /// <summary>
        /// 电量数据
        /// </summary>
        /// <returns></returns>
        public static DataTable GetElectricQuantity()
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            //Config_SqlServerDataFactory.CommandTimeout = 120;
            //int bbb = dataFactory.GetCommandTimeout();
            DataTable result = new DataTable();
            SingleBasicData singleBasicData = SingleBasicData.Creat();
            //singleBasicData.Init("zc_nxjc_byc_byf", "2015-02-12");
            SingleTimeService singleTimeService=SingleTimeService.Creat();
            DataTable mainDatas = GetMainDatas(singleBasicData.OrganizationId, dataFactory);
            string sqlStr = @"SELECT LTRIM(RTRIM(A.VariableId)) AS VariableID,A.OrganizationID,SUM(A.FormulaValue) AS FormulaValue
                                FROM [{0}].[dbo].HistoryFormulaValue AS A
                                WHERE 
                                LEN(LTRIM(RTRIM(A.VariableId))) > 0  ---LTRIM去掉左边空格，RTRIM去掉右边空格
                                AND
                                ({1})
                                GROUP BY LTRIM(RTRIM(A.VariableId)),A.OrganizationID
                            UNION ALL
                            SELECT LTRIM(RTRIM(A.VariableId)) AS VariableID,A.OrganizationID,SUM(A.FormulaValue) AS FormulaValue
                                FROM [{2}].[dbo].HistoryMainMachineFormulaValue AS A
                                WHERE 
                                LEN(LTRIM(RTRIM(A.VariableId))) > 0  ---LTRIM去掉左边空格，RTRIM去掉右边空格
                                AND
                                ({3})
                                GROUP BY LTRIM(RTRIM(A.VariableId)),A.OrganizationID";
            string[] array = {singleTimeService.PeakTimeCriterion,singleTimeService.MorePeakTimeCriterion,
                                 singleTimeService.ValleyTimeCriterion,singleTimeService.MoreValleyTimeCriterion,singleTimeService.FlatTimeCriterion };
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("Peak", singleTimeService.PeakTimeCriterion);
            dictionary.Add("MorePeak", singleTimeService.MorePeakTimeCriterion);
            dictionary.Add("Valley", singleTimeService.ValleyTimeCriterion);
            dictionary.Add("MoreValley", singleTimeService.MoreValleyTimeCriterion);
            dictionary.Add("Flat", singleTimeService.FlatTimeCriterion);

            //foreach (string item in dictionary.Keys)
            //{
            //    DataRow row = result.NewRow();
            //    //row[item]=
            //}
            //峰期
            DataTable sourceP =new DataTable();
            try
            {
                //string mm = string.Format(sqlStr, singleBasicData.AmmeterName, singleTimeService.PeakTimeCriterion,
                //    singleBasicData.AmmeterName, singleTimeService.PeakTimeCriterion);
                sourceP = dataFactory.Query(string.Format(sqlStr, singleBasicData.AmmeterName, singleTimeService.PeakTimeCriterion,
                    singleBasicData.AmmeterName, singleTimeService.PeakTimeCriterion));
            }
            catch
            {
                sourceP = dataFactory.Query(string.Format(sqlStr, singleBasicData.AmmeterName, singleTimeService.PeakTimeCriterion,
                    singleBasicData.AmmeterName, singleTimeService.PeakTimeCriterion));
            }
            //尖峰期
            DataTable sourceMP=new DataTable();
            if (singleTimeService.MorePeakTimeCriterion != "1=0")
            {
                try
                {
                    sourceMP = dataFactory.Query(string.Format(sqlStr, singleBasicData.AmmeterName, singleTimeService.MorePeakTimeCriterion,
                        singleBasicData.AmmeterName, singleTimeService.MorePeakTimeCriterion));
                }
                catch 
                {
                    sourceMP = dataFactory.Query(string.Format(sqlStr, singleBasicData.AmmeterName, singleTimeService.MorePeakTimeCriterion,
                        singleBasicData.AmmeterName, singleTimeService.MorePeakTimeCriterion));
                }
            }
            //谷期
            DataTable sourceV =new DataTable();
            try
            {
                sourceV = dataFactory.Query(string.Format(sqlStr, singleBasicData.AmmeterName, singleTimeService.ValleyTimeCriterion,
                    singleBasicData.AmmeterName, singleTimeService.ValleyTimeCriterion));
            }
            catch 
            {
                sourceV = dataFactory.Query(string.Format(sqlStr, singleBasicData.AmmeterName, singleTimeService.ValleyTimeCriterion,
                    singleBasicData.AmmeterName, singleTimeService.ValleyTimeCriterion));
            }
            //深谷期
            DataTable sourceMV=new DataTable();
            try
            {
                if (singleTimeService.MoreValleyTimeCriterion != "1=0")
                    sourceMV = dataFactory.Query(string.Format(sqlStr, singleBasicData.AmmeterName, singleTimeService.MoreValleyTimeCriterion,
                        singleBasicData.AmmeterName, singleTimeService.MoreValleyTimeCriterion));
            }
            catch 
            {
                sourceMV = dataFactory.Query(string.Format(sqlStr, singleBasicData.AmmeterName, singleTimeService.MoreValleyTimeCriterion,
                        singleBasicData.AmmeterName, singleTimeService.MoreValleyTimeCriterion));
            }
            //峰期
            DataTable sourceF =new DataTable();
            try
            {
                sourceF = dataFactory.Query(string.Format(sqlStr, singleBasicData.AmmeterName, singleTimeService.FlatTimeCriterion,
                    singleBasicData.AmmeterName, singleTimeService.FlatTimeCriterion));
            }
            catch 
            {
                sourceF = dataFactory.Query(string.Format(sqlStr, singleBasicData.AmmeterName, singleTimeService.FlatTimeCriterion,
                    singleBasicData.AmmeterName, singleTimeService.FlatTimeCriterion));
            }
            //甲班
            DataTable first = new DataTable();
            try
            {
                first = dataFactory.Query(string.Format(sqlStr, singleBasicData.AmmeterName, singleTimeService.FirstTimeCriterion,
                    singleBasicData.AmmeterName, singleTimeService.FirstTimeCriterion));
            }
            catch 
            {
                first = dataFactory.Query(string.Format(sqlStr, singleBasicData.AmmeterName, singleTimeService.FirstTimeCriterion,
                    singleBasicData.AmmeterName, singleTimeService.FirstTimeCriterion));
            }
            //乙班
            DataTable second = new DataTable();
            try
            {
                second = dataFactory.Query(string.Format(sqlStr, singleBasicData.AmmeterName, singleTimeService.SecondTimeCriterion,
                    singleBasicData.AmmeterName, singleTimeService.SecondTimeCriterion));
            }
            catch 
            {
                second = dataFactory.Query(string.Format(sqlStr, singleBasicData.AmmeterName, singleTimeService.SecondTimeCriterion,
                    singleBasicData.AmmeterName, singleTimeService.SecondTimeCriterion));
            }
            //丙班
            DataTable third = new DataTable();
            try
            {
                third = dataFactory.Query(string.Format(sqlStr, singleBasicData.AmmeterName, singleTimeService.ThirdTimeCriterion,
                singleBasicData.AmmeterName, singleTimeService.ThirdTimeCriterion));
            }
            catch 
            {
                third = dataFactory.Query(string.Format(sqlStr, singleBasicData.AmmeterName, singleTimeService.ThirdTimeCriterion,
                singleBasicData.AmmeterName, singleTimeService.ThirdTimeCriterion));
            }

            Dictionary<string, DataTable> myDictionary = new Dictionary<string, DataTable>();
            myDictionary.Add("Peak",sourceP);
            myDictionary.Add("MorePeak",sourceMP);
            myDictionary.Add("Valley", sourceV);
            myDictionary.Add("MoreValley", sourceMV);
            myDictionary.Add("Flat", sourceF);
            myDictionary.Add("First", first);
            myDictionary.Add("Second", second);
            myDictionary.Add("Third", third);

            result = singleBasicData.BalanceTable.Clone();//克隆balance_Energy表结构
            foreach (DataRow dr in mainDatas.Rows)
            {
                DataRow row = result.NewRow();
                row["VariableItemId"] = Guid.NewGuid().ToString();
                row["VariableId"] = dr["VariableID"];  //+ "_ElectricityQuantity";
                row["VariableName"] = dr["Name"];
                row["PublicVariableId"] = row["KeyId"] = singleBasicData.KeyId;
                row["OrganizationID"] = dr["OrganizationID"].ToString().Trim();
                row["ValueType"] = "ElectricityQuantity";
                //row["Peak"] = dr["FormulaValue"];
                result.Rows.Add(row);
            }
            for(int i=0;i<result.Rows.Count;i++)
            {
                DataRow curRow = result.Rows[i];
                foreach (string item in myDictionary.Keys.ToArray())
                {
                    string variable = curRow["VariableId"].ToString().Trim();
                    string organizationId = curRow["OrganizationID"].ToString().Trim();
                    if (myDictionary[item].Rows.Count == 0)
                    {
                        curRow[item] = 0;
                    }
                    else
                    {
                        //当前峰谷平甲乙丙班数据源
                        DataTable curTable=myDictionary[item];
                        //如果当前行的VariableId，OrganizationID分别于峰谷平，甲乙丙班的相等
                        if (i<curTable.Rows.Count&& variable == curTable.Rows[i]["VariableId"].ToString().Trim() && organizationId == curTable.Rows[i]["OrganizationID"].ToString().Trim())
                        {
                            curRow[item] = curTable.Rows[i]["FormulaValue"];
                        }
                        else//如果不相等则SELECT
                        {
                            DataRow[] rows = myDictionary[item].Select(string.Format("VariableId='{0}' AND OrganizationID='{1}'", variable, organizationId));
                            if (rows.Count() == 1)
                                curRow[item] = rows[0]["FormulaValue"];
                            else
                                curRow[item] = 0;
                        }
                    }
                }
                curRow["VariableId"] = curRow["VariableID"] + "_ElectricityQuantity";
                curRow["TotalPeakValleyFlat"] = Convert.ToDecimal(curRow["First"]) + Convert.ToDecimal(curRow["Second"]) + Convert.ToDecimal(curRow["Third"]);           
               // result.Rows[i]["TotalPeakValleyFlat"] = Convert.ToDecimal(result.Rows[i]["First"]) + Convert.ToDecimal(result.Rows[i]["Second"]) + Convert.ToDecimal(result.Rows[i]["Third"]);
            }
            foreach (DataRow dr in result.Rows)
            {
                string[] arrayFields = { "TotalPeakValleyFlat", "MorePeak", "Peak", "Valley", "MoreValley", "Flat", "First", "Second", "Third" };
                foreach (string field in arrayFields)
                {
                    if (field == "TotalPeakValleyFlat")
                    {
                        dr[field] = dr[field + "B"] = MyToDecimal(dr["First"]) + MyToDecimal(dr["Second"]) + MyToDecimal(dr["Third"]);
                    }
                    else
                    {
                        dr[field + "B"] = dr[field];
                    }

                }
            }
            //计算日均摊电量
            if ("day" == ConfigService.GetConfig("ShareElectricityCycle"))
            {
                ShareElectricityQuantity.ShareElectricityQuantityDaily.ToShare(result, singleBasicData.KeyId);
            }
            return result;
        }
        private static DataTable GetMainDatas(string myOrganizationId, ISqlServerDataFactory myDataFactory)
        {
            string mySqlStr = @"SELECT B.VariableId,(A.Name+B.Name) AS Name,A.OrganizationID
                                    FROM tz_Formula AS A,formula_FormulaDetail AS B, system_Organization C, system_Organization D
                                    WHERE C.LevelCode like D.LevelCode + '%'
									AND D.OrganizationID = '{0}'
									AND A.KeyID=B.KeyID
                                    AND A.OrganizationID IN (C.OrganizationID)
                                    --AND A.Type=2 
                                    AND A.ENABLE='True' 
                                    AND A.State=0 
                                    AND LEN(LTRIM(RTRIM(B.VariableId))) > 0
                                    AND B.SaveToHistory='TRUE'
                                    ORDER BY B.VariableId,A.OrganizationID";
            DataTable mainDatas = myDataFactory.Query(string.Format(mySqlStr, myOrganizationId));
            return mainDatas;
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

    }
}
