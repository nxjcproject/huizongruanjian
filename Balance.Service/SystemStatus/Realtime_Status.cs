using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Net.NetworkInformation;
using SqlServerDataAdapter;
using Balance.Infrastructure.BasicDate;
using Balance.Infrastructure.Configuration;
using System.Threading;
namespace Balance.Service.SystemStatus
{
    public class Realtime_Status
    {
        private bool _IsClosing;

        private string[] FactoryOrganizationId;
        private string GroupIpAddress;
        public Realtime_Status()
        {
            FactoryOrganizationId = Balance.Infrastructure.Configuration.ConfigService.FactoryOrganizationId;
            GroupIpAddress = Balance.Infrastructure.Configuration.ConfigService.GroupIpAddress;
            Buffer_SystemAlarm.InitSystemAlarm();       //初始化报警缓冲区
        }

        public void StartMonitor()
        {
            Thread m_Thread_SetRealtimeData = new Thread(AlarmMonitor);
            m_Thread_SetRealtimeData.Start();
        }
        private void AlarmMonitor()
        {
            Function_CollectionInterfaceAlarm m_CollectionInterfaceAlarm = new Function_CollectionInterfaceAlarm();
            Function_NetworkAlarm m_NetworkAlarm = new Function_NetworkAlarm();
            Function_SoftwareAlarm m_SoftwareAlarm = new Function_SoftwareAlarm();
            Function_TerminalAlarm m_TerminalAlarm = new Function_TerminalAlarm();
            Function_SaveAndSendAlarm m_SaveAndSendAlarm = new Function_SaveAndSendAlarm();
            Function_UploadSystemStatus m_UploadSystemStatus = new Function_UploadSystemStatus();
            int m_SleepTime = 1000 * 60 * 5;    //5分钟检测一次
            while (IsClosing == false)
            {
                Buffer_SystemAlarm.RefreshUpdateStatus();       //首先刷新一下更新状态
                m_NetworkAlarm.MonitorNetworkStatus(FactoryOrganizationId, GroupIpAddress);  //网络报警
                m_SoftwareAlarm.GetCollectionSoftwareAlarm(FactoryOrganizationId);           //软件报警
                m_CollectionInterfaceAlarm.GetCollectionAlarm(FactoryOrganizationId);        //采集终端报警
                m_TerminalAlarm.GetAmmeterAlarm(FactoryOrganizationId);                      //电表报警
                m_UploadSystemStatus.UploadSystemStatusToWebService(FactoryOrganizationId);                       //报警状态上传到WebService
                m_SaveAndSendAlarm.SaveAlarmStatus(FactoryOrganizationId);                   //存储或者发送报警
                Thread.Sleep(m_SleepTime);
            }
        }
        public bool IsClosing
        {
            get
            {
                return _IsClosing;
            }
            set
            {
                _IsClosing = value;
            }
        } 

        
    }
}
