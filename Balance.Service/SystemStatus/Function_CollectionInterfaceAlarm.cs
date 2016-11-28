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
    /// 数据采集软件报警
    /// </summary>
    public class Function_CollectionInterfaceAlarm
    {
        private ISqlServerDataFactory _dataFactory;
        public Function_CollectionInterfaceAlarm()
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            _dataFactory = new SqlServerDataFactory(connectionString);
        }
        public void GetCollectionAlarm(string[] myFactoryOrganizationId)
        {
            string m_Sql = @"SELECT A.NodeId
                              ,A.OrganizationID as OrganizationId
                              ,A.NodeName
                              ,A.ParentNodeId
                              ,A.NodeType
                              ,A.SwitchModels
                              ,A.IpAddress
                              ,A.RealtimeDataTable
                              ,(case when B.StatusSum = B.StatusCount then '工作正常' else '不能连接' end) as Status
                              ,B.[Timestamp] as Timestamp
                              ,'' as InstanceName
							  ,B.[Servers] as Servers
                          FROM net_DataCollectionNet A, 
                          (Select C.BranchFactory, 
						          C.IP, 
								  C.[Servers], 
								  sum(case when C.Status = '工作正常' then 1 else 0 end) as StatusSum, 
								  count(C.Status) as StatusCount, 
								  min(case when C.Status = '工作正常' then getdate() else [Timestamp] end) as Timestamp
								  from system_StatusNet C
                           group by C.BranchFactory, C.IP, C.[Servers]
                          ) B
                          where A.OrganizationID in ({0})
                              and A.OrganizationID = B.BranchFactory
						      and A.IpAddress = B.IP
						      and A.NodeType = B.[Servers]";
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
                DataTable m_CollectionInterfaceInfoTable = _dataFactory.Query(m_Sql);
                if (m_CollectionInterfaceInfoTable != null)
                {
                    for (int i = 0; i < m_CollectionInterfaceInfoTable.Rows.Count; i++)
                    {
                        string m_Status = m_CollectionInterfaceInfoTable.Rows[i]["Status"] == DBNull.Value ? "" : m_CollectionInterfaceInfoTable.Rows[i]["Status"].ToString();
                        if (m_Status != "工作正常")
                        {
                            Model_SystemAlarm m_SystemAlarmGroup = new Model_SystemAlarm();
                            m_SystemAlarmGroup.NodeId = m_CollectionInterfaceInfoTable.Rows[i]["NodeId"].ToString();
                            m_SystemAlarmGroup.Ip = m_CollectionInterfaceInfoTable.Rows[i]["IpAddress"] == DBNull.Value ? "" : m_CollectionInterfaceInfoTable.Rows[i]["IpAddress"].ToString();
                            m_SystemAlarmGroup.OrganizationId = m_CollectionInterfaceInfoTable.Rows[i]["OrganizationId"] == DBNull.Value ? "" : m_CollectionInterfaceInfoTable.Rows[i]["OrganizationId"].ToString();
                            m_SystemAlarmGroup.NodeName = m_CollectionInterfaceInfoTable.Rows[i]["NodeName"] == DBNull.Value ? "" : m_CollectionInterfaceInfoTable.Rows[i]["NodeName"].ToString();
                            m_SystemAlarmGroup.StartTime = m_CollectionInterfaceInfoTable.Rows[i]["Timestamp"] == DBNull.Value ? "" : m_CollectionInterfaceInfoTable.Rows[i]["Timestamp"].ToString();
                            m_SystemAlarmGroup.Type = m_CollectionInterfaceInfoTable.Rows[i]["Servers"] == DBNull.Value ? "" : m_CollectionInterfaceInfoTable.Rows[i]["Servers"].ToString();
                            m_SystemAlarmGroup.NodeType = m_CollectionInterfaceInfoTable.Rows[i]["NodeType"] == DBNull.Value ? "" : m_CollectionInterfaceInfoTable.Rows[i]["NodeType"].ToString();
                            m_SystemAlarmGroup.InstanceName = m_CollectionInterfaceInfoTable.Rows[i]["InstanceName"] == DBNull.Value ? "" : m_CollectionInterfaceInfoTable.Rows[i]["InstanceName"].ToString();
                            m_SystemAlarmGroup.AlarmDescription = m_SystemAlarmGroup.NodeName;
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
