using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Balance.Service.SystemStatus
{
    public class Function_UploadSystemStatus
    {
        public void UploadSystemStatusToWebService(string[] myFactoryOrganizationId)
        {
            try
            {
                ServiceReference_LocalSystemStatus.NetworkMonitorSoapClient m_LocalSystemStatus = new ServiceReference_LocalSystemStatus.NetworkMonitorSoapClient();
                ServiceReference_RemoteSystemStatus.NetworkMonitorSoapClient m_RemoteSystemStatus = new ServiceReference_RemoteSystemStatus.NetworkMonitorSoapClient();
                Dictionary<string, string> m_UploadData = GetUploadData();               //获得上传数据字典
                if (myFactoryOrganizationId != null && m_UploadData != null)
                {
                    string m_CurrentDateTimeString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    for (int i = 0; i < myFactoryOrganizationId.Length; i++)
                    {
                        if (m_UploadData.ContainsKey(myFactoryOrganizationId[i]))          //当该组织机构有问题则发送错误
                        {
                            byte[] m_UploadDataBuffer = DataCompression.Function_DefaultCompressionArray.CompressString(new string[] { m_UploadData[myFactoryOrganizationId[i]] });
                            try
                            {
                                m_LocalSystemStatus.SetNetworkStatus(myFactoryOrganizationId[i], m_CurrentDateTimeString, m_UploadDataBuffer);
                            }
                            catch
                            {

                            }
                            try
                            {
                                m_RemoteSystemStatus.SetNetworkStatus(myFactoryOrganizationId[i], m_CurrentDateTimeString, m_UploadDataBuffer);
                            }
                            catch
                            {

                            }
                        }
                        else                                         //当该组织机构么有问题,则发送空,用来刷新数据
                        {
                            try
                            {
                                m_LocalSystemStatus.SetNetworkStatus(myFactoryOrganizationId[i], m_CurrentDateTimeString, null);
                            }
                            catch
                            {

                            }
                            try
                            {
                                m_RemoteSystemStatus.SetNetworkStatus(myFactoryOrganizationId[i], m_CurrentDateTimeString, null);
                            }
                            catch
                            {

                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }
        private Dictionary<string, string> GetUploadData()
        {
            Dictionary<string, string> m_UploadData = new Dictionary<string, string>();
            if (Buffer_SystemAlarm.SystemAlarmBuffer != null)
            {
                foreach (string myKey in Buffer_SystemAlarm.SystemAlarmBuffer.Keys)
                {
                    if (Buffer_SystemAlarm.SystemAlarmBuffer[myKey].AlarmCount >= Buffer_SystemAlarm.MaxAlarmCount)
                    {
                        string m_OrganizationId = Buffer_SystemAlarm.SystemAlarmBuffer[myKey].OrganizationId;
                        string m_Type = Buffer_SystemAlarm.SystemAlarmBuffer[myKey].Type;
                        string m_DomNodeId = m_OrganizationId + "_" + Buffer_SystemAlarm.SystemAlarmBuffer[myKey].NodeId;
                        if (!m_UploadData.ContainsKey(m_OrganizationId))
                        {
                            m_UploadData.Add(m_OrganizationId, m_DomNodeId + ";" + m_Type + ";0");
                        }
                        else
                        {
                            m_UploadData[m_OrganizationId] = m_UploadData[m_OrganizationId] + "," + m_DomNodeId + ";" + m_Type + ";0";
                        }
                    }
                }
            }
            return m_UploadData;
        }
    }
}
