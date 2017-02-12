using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Net.NetworkInformation;
using SqlServerDataAdapter;
using Balance.Infrastructure.BasicDate;
using Balance.Infrastructure.Configuration;
namespace Balance.Service.SystemStatus
{
    public class Function_SaveAndSendAlarm
    {
        private const string AlarmGroup = "SystemDiagnostics";
        private const string MessageType = "SMS";
        private const string SenderType = "SystemAlarm";
        private ISqlServerDataFactory _dataFactory;
        public Function_SaveAndSendAlarm()
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            _dataFactory = new SqlServerDataFactory(connectionString);
        }
        public void SaveAlarmStatus(string[] myFactoryOrganizationId)
        {
            if (Buffer_SystemAlarm.SystemAlarmBuffer != null)
            {
                DataTable m_SystemAlarmLogTable = GetSystemAlarmLogTable(myFactoryOrganizationId);   //在报警日志里获取正在报警的信息
                if (m_SystemAlarmLogTable != null)
                {
                    DataTable m_UpdateSystemAlarmTable = m_SystemAlarmLogTable.Clone();   //更新数据库表
                    DataTable m_InsertSystemAlarmTable = m_SystemAlarmLogTable.Clone();   //插入数据库表
                    for (int i = 0; i < m_SystemAlarmLogTable.Rows.Count; i++)       //检测以前报警的是否这次依然报警，如果不报警则更新状态
                    {
                        string m_KeyId = m_SystemAlarmLogTable.Rows[i]["AlarmKeyId"] != DBNull.Value ? m_SystemAlarmLogTable.Rows[i]["AlarmKeyId"].ToString() : "";
                        if (!Buffer_SystemAlarm.SystemAlarmBuffer.ContainsKey(m_KeyId))   //当前报警队列中该报警没有继续
                        {
                            DataRow m_NewDataRow = m_UpdateSystemAlarmTable.NewRow();
                            m_NewDataRow.ItemArray = m_SystemAlarmLogTable.Rows[i].ItemArray;
                            m_NewDataRow["EndTime"] = DateTime.Now;
                            m_UpdateSystemAlarmTable.Rows.Add(m_NewDataRow);
                        }
                        /*
                        else if (Buffer_SystemAlarm.SystemAlarmBuffer[m_KeyId].AlarmCount < Buffer_SystemAlarm.MaxAlarmCount)  //当前报警队列该报警没有达到上限值表示该报警没有继续
                        {
                            DataRow m_NewDataRow = m_UpdateSystemAlarmTable.NewRow();
                            m_NewDataRow.ItemArray = m_SystemAlarmLogTable.Rows[i].ItemArray;
                            m_NewDataRow["EndTime"] = DateTime.Now;
                            m_UpdateSystemAlarmTable.Rows.Add(m_NewDataRow);
                        }*/
                    }
                    foreach (string myKey in Buffer_SystemAlarm.SystemAlarmBuffer.Keys)   //当前报警队列中
                    {
                        if (Buffer_SystemAlarm.SystemAlarmBuffer[myKey].AlarmCount >= Buffer_SystemAlarm.MaxAlarmCount)
                        {
                            DataRow[] m_AlarmRowTemp = m_SystemAlarmLogTable.Select(string.Format("AlarmKeyId = '{0}'", myKey));
                            if (m_AlarmRowTemp == null || m_AlarmRowTemp.Length == 0)   //表示数据库中没有该报警记录
                            {
                                //插入新记录
                                DataRow m_NewDataRow = m_InsertSystemAlarmTable.NewRow();
                                string m_AlarmItemId = System.Guid.NewGuid().ToString();
                                m_NewDataRow["AlarmItemId"] = m_AlarmItemId;
                                m_NewDataRow["AlarmGroup"] = AlarmGroup;
                                m_NewDataRow["AlarmKeyId"] = myKey;
                                m_NewDataRow["OrganizationID"] = Buffer_SystemAlarm.SystemAlarmBuffer[myKey].OrganizationId;
                                m_NewDataRow["AlarmTypeId"] = Buffer_SystemAlarm.SystemAlarmBuffer[myKey].Type;
                                m_NewDataRow["StartTime"] = DateTime.Now;
                                m_NewDataRow["EndTime"] = DBNull.Value;
                                m_NewDataRow["AlarmText"] = Buffer_SystemAlarm.SystemAlarmBuffer[myKey].AlarmDescription;
                                m_InsertSystemAlarmTable.Rows.Add(m_NewDataRow);
                            }
                        }
                    }
                    //////////////////更新故障记录状态//////////////////
                    try
                    {
                        if (m_UpdateSystemAlarmTable.Rows.Count > 0)
                        {
                            _dataFactory.Update("system_AlarmLog", m_UpdateSystemAlarmTable, new string[] { "AlarmItemId" });
                            //当报警解除,未发送的短信不再发送
                            UpdateSmsSendTable(m_UpdateSystemAlarmTable);
                        }
                        //////////////////插入新故障记录////////////////////
                        if (m_InsertSystemAlarmTable.Rows.Count > 0)
                        {
                            _dataFactory.Insert("system_AlarmLog", m_InsertSystemAlarmTable, new string[0]);
                            //////////////////新故障记录插入到短信发送列表/////////////////////
                            InsertSmsSendTable(m_InsertSystemAlarmTable);
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
        private DataTable GetSystemAlarmLogTable(string[] myFactoryOrganizationId)
        {
            string m_Sql = @"SELECT A.AlarmItemId
                                  ,A.AlarmGroup
                                  ,A.AlarmKeyId
                                  ,A.OrganizationID
                                  ,A.AlarmTypeId
                                  ,A.StartTime
                                  ,A.EndTime
                                  ,AlarmText
                              FROM system_AlarmLog A
                              where A.EndTime is null
                              and A.AlarmGroup = '{0}'
                              and A.OrganizationID in ({1})";
            string m_OrganizationId = "";
            if (myFactoryOrganizationId != null)
            {
                for (int i = 0; i < myFactoryOrganizationId.Length; i++)
                {
                    if (i == 0)
                    {
                        m_OrganizationId = "'" + myFactoryOrganizationId[i] + "'";
                    }
                    else
                    {
                        m_OrganizationId = m_OrganizationId + ",'" + myFactoryOrganizationId[i] + "'";
                    }
                }
            }
            m_Sql = string.Format(m_Sql, AlarmGroup, m_OrganizationId);
            try
            {
                DataTable m_SystemAlarmLogTable = _dataFactory.Query(m_Sql);
                return m_SystemAlarmLogTable;
            }
            catch
            {
                return null;
            }
        }
        private void InsertSmsSendTable(DataTable myInsertSystemAlarmTable)
        {
            DataTable m_SmsSendInfoTable = GetSmsSendInfo();
            if (myInsertSystemAlarmTable != null && m_SmsSendInfoTable != null)
            {
                string m_AlarmTypeId = "''";
                for (int i = 0; i < myInsertSystemAlarmTable.Rows.Count; i++)
                {
                    if (i == 0)
                    {
                        m_AlarmTypeId = "'" + myInsertSystemAlarmTable.Rows[i]["AlarmTypeId"].ToString() + "'";
                    }
                    else
                    {
                        m_AlarmTypeId = m_AlarmTypeId + ",'" + myInsertSystemAlarmTable.Rows[i]["AlarmTypeId"].ToString() + "'";
                    }
                }
                DataTable m_SendStaffInfoTable = GetSendStaffInfo(m_AlarmTypeId);   //获得发送人员列表
                if (m_SendStaffInfoTable != null)
                {
                    for (int i = 0; i < myInsertSystemAlarmTable.Rows.Count; i++)
                    {
                        for (int j = 0; j < m_SendStaffInfoTable.Rows.Count; j++)
                        {
                            string m_InsertSystemAlarmTypeId = myInsertSystemAlarmTable.Rows[i]["AlarmTypeId"] != DBNull.Value ? myInsertSystemAlarmTable.Rows[i]["AlarmTypeId"].ToString() : "";
                            string m_SendStaffAlarmTypeId = m_SendStaffInfoTable.Rows[j]["AlarmTypeId"] != DBNull.Value ? m_SendStaffInfoTable.Rows[j]["AlarmTypeId"].ToString() : "";
                            if (m_InsertSystemAlarmTypeId != "" && m_SendStaffAlarmTypeId != "" && m_InsertSystemAlarmTypeId == m_SendStaffAlarmTypeId)
                            {
                                DataRow m_NewRow = m_SmsSendInfoTable.NewRow();
                                string m_DelayTime = m_SendStaffInfoTable.Rows[j]["SendDelay"] != DBNull.Value ? m_SendStaffInfoTable.Rows[j]["SendDelay"].ToString() : "";
                                string m_StartTime = m_SendStaffInfoTable.Rows[j]["StartTime"] != DBNull.Value ? m_SendStaffInfoTable.Rows[j]["StartTime"].ToString() : "";
                                string m_EndTime = m_SendStaffInfoTable.Rows[j]["EndTime"] != DBNull.Value ? m_SendStaffInfoTable.Rows[j]["EndTime"].ToString() : "";
                                m_NewRow["SenderKeyId"] = myInsertSystemAlarmTable.Rows[i]["AlarmItemId"].ToString();
                                m_NewRow["SenderType"] = SenderType;
                                m_NewRow["GroupKey1"] = m_SendStaffInfoTable.Rows[j]["OrganizationName"] != DBNull.Value ? m_SendStaffInfoTable.Rows[j]["OrganizationName"].ToString() : "";
                                m_NewRow["GroupKey2"] = m_SendStaffInfoTable.Rows[j]["AlarmTypeName"] != DBNull.Value ? m_SendStaffInfoTable.Rows[j]["AlarmTypeName"].ToString() : "";
                                m_NewRow["GroupKey3"] = DBNull.Value;
                                m_NewRow["PhoneNumber"] = m_SendStaffInfoTable.Rows[j]["PhoneNumber"] != DBNull.Value ? m_SendStaffInfoTable.Rows[j]["PhoneNumber"].ToString() : "";
                                m_NewRow["SendCount"] = 0;
                                m_NewRow["CreateTime"] = DateTime.Now;
                                m_NewRow["OrderSendTime"] = GetSendTime(m_DelayTime, m_StartTime, m_EndTime);
                                m_NewRow["AlarmText"] = myInsertSystemAlarmTable.Rows[i]["AlarmText"] != DBNull.Value ? myInsertSystemAlarmTable.Rows[i]["AlarmText"].ToString() : "";
                                m_NewRow["State"] = 0;
                                m_SmsSendInfoTable.Rows.Add(m_NewRow);
                            }
                        }
                    }
                    if (m_SmsSendInfoTable.Rows.Count > 0)
                    {
                        _dataFactory.Insert("terminal_SmsSendInfo", m_SmsSendInfoTable, new string[] { "SendItemId" });
                    }
                }
            }
        }
        private void UpdateSmsSendTable(DataTable myUpdateSystemAlarmTable)
        {
            string m_UpdateAlarmItemIds = "''";
            for (int i = 0; i < myUpdateSystemAlarmTable.Rows.Count; i++)
            {
                if (i == 0)
                {
                    m_UpdateAlarmItemIds = "'" + myUpdateSystemAlarmTable.Rows[i]["AlarmItemId"].ToString() + "'";
                }
                else
                {
                    m_UpdateAlarmItemIds = m_UpdateAlarmItemIds + ",'" + myUpdateSystemAlarmTable.Rows[i]["AlarmItemId"].ToString() + "'";
                }
            }
            string m_Sql = @"update terminal_SmsSendInfo 
                           set State = {3}
                           where SenderKeyId in ({0}) and SenderType = '{1}' and State = {2}";

            m_Sql = string.Format(m_Sql, m_UpdateAlarmItemIds, SenderType, "0", "80");
            try
            {
                _dataFactory.ExecuteSQL(m_Sql);
            }
            catch
            {
            }
        }
        private DataTable GetSendStaffInfo(string myAlarmTypeIdString)
        {
            string m_Sql = @"SELECT A.ContrastItemId
                                  ,A.StaffInfoItemId
	                              ,A.OrganizationID
                                  ,D.Name as OrganizationName
                                  ,A.MessageType
                                  ,A.AlarmTypeId
	                              ,C.AlarmTypeName
	                              ,(case when A.SendDelay is not null then A.SendDelay when C.SendDelay is not null then C.SendDelay else 0 end) as SendDelay
                                  ,(case when A.StartTime is null and A.EndTime is null then C.StartTime else A.StartTime end) as StartTime
	                              ,(case when A.StartTime is null and A.EndTime is null then C.EndTime else A.EndTime end) as EndTime
	                              ,B.PhoneNumber
                                  ,A.AlarmText
                                  ,A.Enabled
                              FROM terminal_SystemAlarmContrast A
                              left join system_SystemAlarmType C on A.AlarmTypeId = C.AlarmTypeId
                              left join system_Organization D on A.OrganizationID = D.OrganizationID
                              , system_StaffInfo B 
                              where A.StaffInfoItemId = B.StaffInfoItemId
                              and A.OrganizationID = B.OrganizationID
                              and A.Enabled = 1
                              and B.Enabled = 1
                              and A.MessageType = '{0}'
                              and A.AlarmTypeId in ({1})";
            m_Sql = string.Format(m_Sql, MessageType, myAlarmTypeIdString);
            try
            {
                DataTable m_SendStaffInfoTable = _dataFactory.Query(m_Sql);
                return m_SendStaffInfoTable;
            }
            catch
            {
                return null;
            }
        }
        private DataTable GetSmsSendInfo()
        {
            string m_Sql = @"SELECT top 0 * from terminal_SmsSendInfo";
            try
            {
                DataTable m_SmsSendInfoTable = _dataFactory.Query(m_Sql);
                return m_SmsSendInfoTable;
            }
            catch
            {
                return null;
            }
        }
        private DateTime GetSendTime(string myDelayTime, string StartDateTime, string EndDateTime)
        {
            try
            {
                int m_DelayMinutes = myDelayTime != "" ? Int32.Parse(myDelayTime) : 0;
                DateTime m_DalayDateTime = DateTime.Now.AddMinutes(m_DelayMinutes);
                if (StartDateTime != "" && EndDateTime != "")      //当有开始和结束时间
                {
                    DateTime m_StartTime = DateTime.Parse(m_DalayDateTime.Year.ToString() + "-" + m_DalayDateTime.Month.ToString() + "-" + m_DalayDateTime.Day
                                                                  + " " + StartDateTime + ".000");
                    DateTime m_EndTime = DateTime.Parse(m_DalayDateTime.Year.ToString() + "-" + m_DalayDateTime.Month.ToString() + "-" + m_DalayDateTime.Day
                                                                  + " " + EndDateTime + ".999");
                    if (m_StartTime > m_EndTime)    //当开始时间比结束时间大,则需要把结束时间向后延1天
                    {
                        m_EndTime = m_EndTime.AddDays(1);
                    }
                    if (m_DalayDateTime < m_StartTime)         //当小于开始时间,则返回开始时间
                    {
                        return m_StartTime;
                    }
                    else if (m_DalayDateTime > m_EndTime)      //当晚于结束时间,则返回第二天开始时间
                    {
                        return m_StartTime.AddDays(1);
                    }
                    else                                  //返回延迟时间
                    {
                        return m_DalayDateTime;
                    }
                }
                else if (StartDateTime != "" && EndDateTime == "")    //当有开始时间没有结束时间,就认为是到第二天0点前结束
                {
                    DateTime m_StartTime = DateTime.Parse(m_DalayDateTime.Year.ToString() + "-" + m_DalayDateTime.Month.ToString() + "-" + m_DalayDateTime.Day
                                                                  + " " + StartDateTime + ".000");
                    DateTime m_EndTime = new DateTime(m_DalayDateTime.Year, m_DalayDateTime.Month, m_DalayDateTime.Day, 23, 59, 59, 999);

                    if (m_DalayDateTime < m_StartTime)         //当小于开始时间,则返回开始时间
                    {
                        return m_StartTime;
                    }
                    else if (m_DalayDateTime > m_EndTime)      //当晚于结束时间,则返回第二天开始时间
                    {
                        return m_StartTime.AddDays(1);
                    }
                    else                                  //返回延迟时间
                    {
                        return m_DalayDateTime;
                    }
                }
                else if (StartDateTime == "" && EndDateTime != "")    //当没有开始时间只有结束时间,就认为到当天0点开始
                {
                    DateTime m_StartTime = new DateTime(m_DalayDateTime.Year, m_DalayDateTime.Month, m_DalayDateTime.Day, 0, 0, 0, 0);
                    DateTime m_EndTime = DateTime.Parse(m_DalayDateTime.Year.ToString() + "-" + m_DalayDateTime.Month.ToString() + "-" + m_DalayDateTime.Day
                                              + " " + EndDateTime + ".999");
                    if (m_DalayDateTime < m_StartTime)         //当小于开始时间,则返回开始时间
                    {
                        return m_StartTime;
                    }
                    else if (m_DalayDateTime > m_EndTime)      //当晚于结束时间,则返回第二天开始时间
                    {
                        return m_StartTime.AddDays(1);
                    }
                    else                                  //返回延迟时间
                    {
                        return m_DalayDateTime;
                    }
                }
                else                                                 //表示没有时间限制
                {
                    return m_DalayDateTime;
                }
            }
            catch
            {
                return DateTime.Now;
            }
        }
    }
}
