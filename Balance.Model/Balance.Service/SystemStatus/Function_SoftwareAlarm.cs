using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using SqlServerDataAdapter;
using Balance.Infrastructure.BasicDate;
using Balance.Infrastructure.Configuration;
namespace Balance.Service.SystemStatus
{
    /// <summary>
    /// 软件报警
    /// </summary>
    public class Function_SoftwareAlarm
    {
        private ISqlServerDataFactory _dataFactory;
        public Function_SoftwareAlarm()
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            _dataFactory = new SqlServerDataFactory(connectionString);
        }
        public void GetCollectionSoftwareAlarm(string[] myFactoryOrganizationId)
        {
            string m_Sql = @"SELECT A.NodeId
                              ,A.OrganizationID as OrganizationId
                              ,A.NodeName
                              ,A.ParentNodeId
                              ,A.NodeType
                              ,A.SwitchModels
                              ,A.IpAddress
                              ,A.RealtimeDataTable
                              ,B.Status
                              ,B.[Timestamp] as Timestamp
                              ,B.InstanceName
                              ,B.[Servers]
                              ,B.Remark
                          FROM net_DataCollectionNet A, system_StatusNet B
                          where A.OrganizationID in ({0})
                          and A.OrganizationID = B.BranchFactory
						  and A.IpAddress = B.IP
						  and A.NodeType = 'DataComputer'
						  and B.[Servers] = 'Software'";
            string m_FactoryOrganizationId = "";
            for (int i = 0; i < myFactoryOrganizationId.Length; i++)
            {
                if (i == 0)
                {
                    m_FactoryOrganizationId = "'" + myFactoryOrganizationId[i] + "'";
                }
                else
                {
                    m_FactoryOrganizationId = m_FactoryOrganizationId + ",'" + myFactoryOrganizationId[i] + "'";
                }
            }
            m_Sql = string.Format(m_Sql, m_FactoryOrganizationId);
            try
            {
                DataTable m_SoftwareInfoTable = _dataFactory.Query(m_Sql);
                if (m_SoftwareInfoTable != null)
                {
                    for (int i = 0; i < m_SoftwareInfoTable.Rows.Count; i++)
                    {
                        bool m_Status = false;
                        if (m_SoftwareInfoTable.Rows[i]["Timestamp"] != DBNull.Value)
                        {
                            DateTime m_TimeStamp = DateTime.Parse(m_SoftwareInfoTable.Rows[i]["Timestamp"].ToString());
                            if (m_TimeStamp.AddMinutes(10) >= DateTime.Now)      //如果记录的时间与当前时间间隔不到10分钟表示数据采集软件运行正常
                            {
                                m_Status = true;
                            }
                        }

                        if (m_Status == false)
                        {
                            Model_SystemAlarm m_SystemAlarmGroup = new Model_SystemAlarm();
                            m_SystemAlarmGroup.NodeId = m_SoftwareInfoTable.Rows[i]["NodeId"].ToString();
                            m_SystemAlarmGroup.Ip = m_SoftwareInfoTable.Rows[i]["IpAddress"] == DBNull.Value ? "" : m_SoftwareInfoTable.Rows[i]["IpAddress"].ToString();
                            m_SystemAlarmGroup.OrganizationId = m_SoftwareInfoTable.Rows[i]["OrganizationId"] == DBNull.Value ? "" : m_SoftwareInfoTable.Rows[i]["OrganizationId"].ToString();
                            m_SystemAlarmGroup.NodeName = m_SoftwareInfoTable.Rows[i]["NodeName"] == DBNull.Value ? "" : m_SoftwareInfoTable.Rows[i]["NodeName"].ToString();
                            m_SystemAlarmGroup.StartTime = m_SoftwareInfoTable.Rows[i]["Timestamp"] == DBNull.Value ? "" : m_SoftwareInfoTable.Rows[i]["Timestamp"].ToString();
                            m_SystemAlarmGroup.Type = m_SoftwareInfoTable.Rows[i]["Servers"] == DBNull.Value ? "" : m_SoftwareInfoTable.Rows[i]["Servers"].ToString();
                            m_SystemAlarmGroup.NodeType = m_SoftwareInfoTable.Rows[i]["NodeType"] == DBNull.Value ? "" : m_SoftwareInfoTable.Rows[i]["NodeType"].ToString();
                            m_SystemAlarmGroup.InstanceName = m_SoftwareInfoTable.Rows[i]["InstanceName"] == DBNull.Value ? "" : m_SoftwareInfoTable.Rows[i]["InstanceName"].ToString();
                            m_SystemAlarmGroup.AlarmDescription = m_SystemAlarmGroup.Ip;
                            Buffer_SystemAlarm.SetAlarm(m_SystemAlarmGroup.Type + "_" + m_SystemAlarmGroup.NodeType + "_" + m_SystemAlarmGroup.NodeId + "_" + m_SystemAlarmGroup.InstanceName, m_SystemAlarmGroup);
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}
