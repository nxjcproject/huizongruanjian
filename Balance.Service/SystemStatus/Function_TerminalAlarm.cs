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
    public class Function_TerminalAlarm
    {
        private ISqlServerDataFactory _dataFactory;
        public Function_TerminalAlarm()
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            _dataFactory = new SqlServerDataFactory(connectionString);
        }
        public void GetAmmeterAlarm(string[] myFactoryOrganizationId)
        {
            DataTable m_DataBaseInfoTable = GetDataBaseByOrganizationId(myFactoryOrganizationId);
            if (m_DataBaseInfoTable != null)
            {
                string m_SqlTemplate = @"Select N.NodeId
                                  ,N.NodeName
                                  ,N.ParentNodeId
                                  ,N.NodeType
                                  ,N.NodeType as Servers
                                  ,N.SwitchModels
                                  ,N.InstanceName
                                  ,M.StatusCount
                                  ,M.StatusSum
								  ,M.IpAddress
								  ,M.OrganizationID as OrganizationId
                                  ,(case when M.StatusCount - M.StatusSum > 0 then 0 else 1 end) as Status
                              from (
	                              Select A.IpAddress
	                              ,A.OrganizationID
                                  ,A.ElectricRoom
	                              ,count(A.Status) as StatusCount
	                              ,sum(case when A.Status = '正常读取' then 1 else 0 end) as StatusSum
	                              from {0}.dbo.AmmeterContrast A
	                              where EnabledFlag = 1
	                              group by A.OrganizationID,A.ElectricRoom,A.IpAddress) M, net_DataCollectionNet N
                              where N.NodeType = 'Ammeter'
                              and M.IpAddress = N.IpAddress
                              and M.OrganizationID = N.OrganizationID
                              and M.ElectricRoom = N.NodeName
                              and N.OrganizationID = '{1}'";
                //A.IpAddress is not null and A.IpAddress <> '' and 
                for (int i = 0; i < myFactoryOrganizationId.Length; i++)
                {
                    DataRow[] m_DataBaseRow = m_DataBaseInfoTable.Select(string.Format("OrganizationId = '{0}'", myFactoryOrganizationId[i]));
                    if (m_DataBaseRow.Length > 0)
                    {
                        string m_Sql = m_SqlTemplate;
                        m_Sql = string.Format(m_Sql, m_DataBaseRow[0]["MeterDatabase"].ToString(), myFactoryOrganizationId[i]);
                        try
                        {
                            DataTable m_AmmeterInfoTable = _dataFactory.Query(m_Sql);
                            if (m_AmmeterInfoTable != null)
                            {
                                for (int j = 0; j < m_AmmeterInfoTable.Rows.Count; j++)
                                {
                                    string m_Status = m_AmmeterInfoTable.Rows[j]["Status"] == DBNull.Value ? "0" : m_AmmeterInfoTable.Rows[j]["Status"].ToString();
                                    if (m_Status != "1")
                                    {
                                        Model_SystemAlarm m_SystemAlarmGroup = new Model_SystemAlarm();
                                        m_SystemAlarmGroup.NodeId = m_AmmeterInfoTable.Rows[j]["NodeId"].ToString();
                                        m_SystemAlarmGroup.Ip = m_AmmeterInfoTable.Rows[j]["IpAddress"] == DBNull.Value ? "" : m_AmmeterInfoTable.Rows[j]["IpAddress"].ToString();
                                        m_SystemAlarmGroup.OrganizationId = m_AmmeterInfoTable.Rows[j]["OrganizationId"] == DBNull.Value ? "" : m_AmmeterInfoTable.Rows[j]["OrganizationId"].ToString();
                                        m_SystemAlarmGroup.NodeName = m_AmmeterInfoTable.Rows[j]["NodeName"] == DBNull.Value ? "" : m_AmmeterInfoTable.Rows[j]["NodeName"].ToString();
                                        m_SystemAlarmGroup.StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        m_SystemAlarmGroup.Type = m_AmmeterInfoTable.Rows[j]["Servers"] == DBNull.Value ? "" : m_AmmeterInfoTable.Rows[j]["Servers"].ToString() + "S";
                                        m_SystemAlarmGroup.NodeType = m_AmmeterInfoTable.Rows[j]["NodeType"] == DBNull.Value ? "" : m_AmmeterInfoTable.Rows[j]["NodeType"].ToString();
                                        m_SystemAlarmGroup.InstanceName = m_AmmeterInfoTable.Rows[j]["InstanceName"] == DBNull.Value ? "" : m_AmmeterInfoTable.Rows[j]["InstanceName"].ToString();
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
        }
        private DataTable GetDataBaseByOrganizationId(string[] myFactoryOrganizationId)
        {
            string m_Sql = @"SELECT A.DatabaseID as DatabaseId
                              ,A.MeterDatabase as MeterDatabase
	                          ,B.OrganizationID as OrganizationId
                              FROM system_Database A, system_Organization B
                              where B.OrganizationID in ({0})
                              and B.DatabaseID = A.DatabaseID";
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
                DataTable m_DataBaseTable = _dataFactory.Query(m_Sql);
                return m_DataBaseTable;
            }
            catch
            {
                return null;
            }
        }
    }
}
