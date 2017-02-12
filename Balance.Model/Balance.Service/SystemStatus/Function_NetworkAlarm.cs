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
    /// <summary>
    /// 网络报警
    /// </summary>
    public class Function_NetworkAlarm
    {
        private ISqlServerDataFactory _dataFactory;
        public Function_NetworkAlarm()
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            _dataFactory = new SqlServerDataFactory(connectionString);
        }
        public void MonitorNetworkStatus(string[] myFactoryOrganizationId, string myGroupIpAddress)
        {
            
            //分厂服务器与数据采集计算机之间的网络
            string m_Sql = @"SELECT A.NodeId
                              ,A.OrganizationID as OrganizationId
                              ,A.NodeName
                              ,A.ParentNodeId
                              ,A.NodeType
                              ,A.SwitchModels
                              ,A.IpAddress 
                              ,A.RealtimeDataTable
                              ,A.InstanceName
                          FROM net_DataCollectionNet A
                          where A.OrganizationID in ({0})
                          and (A.NodeType = '{1}' or A.NodeType = '{2}')";
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
            m_Sql = string.Format(m_Sql, m_FactoryOrganizationId, "DataComputer", "FactoryServer");
            try
            {
                DataTable m_DataBaseInfoTable = _dataFactory.Query(m_Sql);
                if (m_DataBaseInfoTable != null)
                {
                    for (int i = 0; i < m_DataBaseInfoTable.Rows.Count; i++)
                    {
                        bool m_DataComputerNetworkStatus = false;
                        string m_IpAddress = m_DataBaseInfoTable.Rows[i]["IpAddress"] == DBNull.Value ? "" : m_DataBaseInfoTable.Rows[i]["IpAddress"].ToString();
                        if (m_DataBaseInfoTable.Rows[i]["NodeType"].ToString() == "FactoryServer") //当前节点是分厂服务器时,实际上ping的是分厂服务器到集团服务器的网络
                        {
                            m_DataComputerNetworkStatus = GetNetworkStatus(myGroupIpAddress);
                        }
                        else
                        {
                            m_DataComputerNetworkStatus = GetNetworkStatus(m_IpAddress);  //当前节点是数据采集计算机时，实际上ping的是分厂服务器到采集计算机的网络
                        }
                        if (m_DataComputerNetworkStatus == false)
                        {
                            Model_SystemAlarm m_SystemAlarmGroup = new Model_SystemAlarm();
                            m_SystemAlarmGroup.NodeId = m_DataBaseInfoTable.Rows[i]["NodeId"].ToString();
                            m_SystemAlarmGroup.Ip = m_IpAddress;
                            m_SystemAlarmGroup.OrganizationId = m_DataBaseInfoTable.Rows[i]["OrganizationId"].ToString();
                            m_SystemAlarmGroup.NodeName = m_DataBaseInfoTable.Rows[i]["NodeName"].ToString();
                            m_SystemAlarmGroup.StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            m_SystemAlarmGroup.Type = "Network";
                            m_SystemAlarmGroup.NodeType = m_DataBaseInfoTable.Rows[i]["NodeType"].ToString();
                            m_SystemAlarmGroup.InstanceName = m_DataBaseInfoTable.Rows[i]["InstanceName"] == DBNull.Value ? "" : m_DataBaseInfoTable.Rows[i]["InstanceName"].ToString();
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
        /// <summary>
        /// 判断网络状态
        /// </summary>
        /// <param name="myIpAddress">IP地址</param>
        /// <returns>通断状态</returns>
        private bool GetNetworkStatus(string myIpAddress)
        {
            if (myIpAddress != null)
            {
                string m_IpAddress = myIpAddress.Trim();
                try
                {
                    Ping m_Ping = new Ping();
                    PingReply m_PingReply = m_Ping.Send(m_IpAddress);
                    if (m_PingReply.Status == IPStatus.Success)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        private DataTable GetStatusNetTable()
        {
            DataTable m_StatusNetTable = new DataTable();
            m_StatusNetTable.Columns.Add("ID", typeof(string));
            m_StatusNetTable.Columns.Add("Company", typeof(string));
            m_StatusNetTable.Columns.Add("BranchFactory", typeof(string));
            m_StatusNetTable.Columns.Add("IP", typeof(string));
            m_StatusNetTable.Columns.Add("Servers", typeof(string));
            m_StatusNetTable.Columns.Add("Status", typeof(string));
            m_StatusNetTable.Columns.Add("Timestamp", typeof(string));
            m_StatusNetTable.Columns.Add("Remark", typeof(string));
            m_StatusNetTable.Columns.Add("Type", typeof(string));
            return m_StatusNetTable;
        }
    }
}
