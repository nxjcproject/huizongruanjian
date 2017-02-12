using Balance.Infrastructure.Configuration;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Replication;
using SqlServerDataAdapter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Balance.Service.SyncStatus
{
    public class SyncStatusToGroup
    {
        private bool _SyncSwitch;
        private Int32 _monitorCycle;

        public SyncStatusToGroup() {
            _SyncSwitch = Convert.ToBoolean(ConfigService.GetConfig("SyncSwitch"));
            _monitorCycle = Convert.ToInt32(ConfigService.GetConfig("monitorCycle"));       
        }
        public void StartSubscriptionMonitor()
        {
            Thread m_Thread_SubscriptionMonitor = new Thread(new ThreadStart(SetSyncStatusToGroup));
            m_Thread_SubscriptionMonitor.Start();

        }
        private void SetSyncStatusToGroup()
        {
           // int monitorCycle=3000;    //刷新周期
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            string remoteConnectionString = ConnectionStringFactory.GroupNXJCConnectionString;
            // string localIp = GetAddressIP();
            string instanceConnectionString = InstanceConnectionString()[0];
            string serverId = InstanceConnectionString()[1];
            while (_SyncSwitch)
            {
                //获取表结构
                DataTable setSubscriptionStatus = SubscriptionStatusTableStructure();
                DataTable setTzSubscriptionStatus=tzSubscriptionStatusTableStructure();
                //获取订阅状态表
                DataTable tb_SyncStatus = HelpSubscription();
                //获取当前时间
                DateTime nowTime = DateTime.Now;
                int alarmCount = 0;
                int insertCount = 0;
                int repiredCount = 0;
                int tbSyncStatusCount=tb_SyncStatus.Rows.Count;
                bool tzAlarm=false;
                if (tb_SyncStatus != null && tbSyncStatusCount > 0)
                {
                    if (!instanceConnectionString.Equals(""))   //如果实例连接字符串存在
                    {
                        for (int i = 0; i < tbSyncStatusCount; i++)  //遍历订阅状态表
                        {
                            string subscriberName = tb_SyncStatus.Rows[i]["subscriber"].ToString();//订阅者
                            string subscriptionDbName = tb_SyncStatus.Rows[i]["subscriber_db"].ToString();//订阅数据库名称
                            string publicationDbName = tb_SyncStatus.Rows[i]["publisher_db"].ToString();//发布数据库名称
                            string publicationName = tb_SyncStatus.Rows[i]["publication"].ToString();//发布名称

                            int mMonitorranking = Convert.ToInt16(tb_SyncStatus.Rows[i]["monitorranking"]);
                            int mPublication_type = Convert.ToInt16(tb_SyncStatus.Rows[i]["publication_type"]);
                            int mSubtype = Convert.ToInt16(tb_SyncStatus.Rows[i]["subtype"]);

                            int mStatus = Convert.ToInt16(tb_SyncStatus.Rows[i]["status"]);
                            int mWarning = Convert.ToInt16(tb_SyncStatus.Rows[i]["warning"]);
                            string mSubscriber_db = tb_SyncStatus.Rows[i]["subscriber_db"].ToString();

                            string mMaintenanceResult = "";
                            bool mIsAlarm = false;

                            //对监控等级为“错误”和“未运行”的推送事务发布进行重启代理
                            if ((mMonitorranking == 60     //判断监控等级为“错误”
                                || mMonitorranking == 30 || mMonitorranking == 57 || mMonitorranking == 53)  //判断监控等级为“未运行”
                                && mPublication_type == 0    //事务发布
                                && mSubtype == 0)         //推送
                            {   //进行重启代理作业

                                SqlConnection sqlConnection = new SqlConnection(instanceConnectionString.Replace("NXJC", publicationDbName));
                                ServerConnection conn = new ServerConnection(sqlConnection);
                                TransSubscription subscription;
                                try
                                {
                                    //连接到发布者
                                    conn.Connect();
                                    // 初始化推订阅  
                                    subscription = new TransSubscription();
                                    subscription.ConnectionContext = conn;
                                    subscription.DatabaseName = publicationDbName;
                                    subscription.PublicationName = publicationName;
                                    subscription.SubscriptionDBName = subscriptionDbName;
                                    subscription.SubscriberName = subscriberName;

                                    //如果推订阅和作业存在，开始代理作业
                                    if (subscription.LoadProperties() && subscription.AgentJobId != null)
                                    {
                                        subscription.SynchronizeWithJob();
                                        // subscription.StopSynchronizationJob();
                                        mMaintenanceResult = "重启代理作业成功!";
                                        mIsAlarm = false;
                                        repiredCount++;

                                    }
                                    else
                                    {
                                        //如果订阅不存在                               
                                        mMaintenanceResult = "订阅不存在";
                                        mIsAlarm = true;
                                        alarmCount++;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //进行适当的处理（抛出异常）
                                    //throw new ApplicationException("订阅不能够被停止.", ex);
                                    mMaintenanceResult = "重启代理失败！";
                                    mIsAlarm = true;
                                    alarmCount++;
                                }
                                finally
                                {
                                    conn.Disconnect();
                                    setSubscriptionStatus.Rows.Add(System.Guid.NewGuid().ToString(), nowTime, serverId, publicationName, mStatus, 60, mMaintenanceResult, mIsAlarm, mWarning, mSubscriber_db);
                                }
                            }
                            else
                            {
                                //将状态直接写入数据库表repl_SubscriptionStatus  
                                if ((mPublication_type == 0 && mMonitorranking == 60) || (mPublication_type == 0 && mMonitorranking == 30))
                                {
                                    mIsAlarm = true;
                                    alarmCount++;
                                }
                                setSubscriptionStatus.Rows.Add(System.Guid.NewGuid().ToString(), nowTime, serverId, publicationName, mStatus, mMonitorranking, mMaintenanceResult, mIsAlarm, mWarning, mSubscriber_db);
                            }
                        }
                    }
                    else   //如果实例连接字符串不存在
                    {
                        for (int i = 0; i < tb_SyncStatus.Rows.Count; i++)  //遍历订阅状态表
                        {

                            string subscriberName = tb_SyncStatus.Rows[i]["subscriber"].ToString();//订阅者
                            string subscriptionDbName = tb_SyncStatus.Rows[i]["subscriber_db"].ToString();//订阅数据库名称
                            string publicationDbName = tb_SyncStatus.Rows[i]["publisher_db"].ToString();//发布数据库名称
                            string publicationName = tb_SyncStatus.Rows[i]["publication"].ToString();//发布名称

                            int mMonitorranking = Convert.ToInt16(tb_SyncStatus.Rows[i]["monitorranking"]);
                            int mPublication_type = Convert.ToInt16(tb_SyncStatus.Rows[i]["publication_type"]);
                            int mSubtype = Convert.ToInt16(tb_SyncStatus.Rows[i]["subtype"]);

                            int mStatus = Convert.ToInt16(tb_SyncStatus.Rows[i]["status"]);
                            int mWarning = Convert.ToInt16(tb_SyncStatus.Rows[i]["warning"]);
                            string mSubscriber_db = tb_SyncStatus.Rows[i]["subscriber_db"].ToString();

                            string mMaintenanceResult = "";
                            bool mIsAlarm = false;

                            if ((mMonitorranking == 60     //判断监控等级为“错误”
                                || mMonitorranking == 30 || mMonitorranking == 57)  //判断监控等级为“未运行”
                                && mPublication_type == 0    //事务发布
                                && mSubtype == 0)         //推送
                            {   //不再尝试重启代理作业，只进行报错
                                mMaintenanceResult = "实例名获取错误";
                                mIsAlarm = true;
                                alarmCount++;
                                setSubscriptionStatus.Rows.Add(System.Guid.NewGuid().ToString(), nowTime, serverId, publicationName, mStatus, 60, mMaintenanceResult, mIsAlarm, mWarning, mSubscriber_db);
                            }
                            else
                            {
                                //将状态直接写入数据库表repl_SubscriptionStatus  
                                if ((mPublication_type == 0 && mMonitorranking == 60) || (mPublication_type == 0 && mMonitorranking == 30))
                                {
                                    mIsAlarm = true;
                                    alarmCount++;
                                }
                                setSubscriptionStatus.Rows.Add(System.Guid.NewGuid().ToString(), nowTime, serverId, publicationName, mStatus, mMonitorranking, mMaintenanceResult, mIsAlarm, mWarning, mSubscriber_db);
                            }
                        }
                    }
                    //将setSubscriptionStatus表插入数据库中
                    ISqlServerDataFactory myDataFactory = new SqlServerDataFactory(remoteConnectionString);
                    string[] columns = { "StatusItemId", "vDate", "serverId", "publication", "status", "monitorranking", "MaintenanceResult", "IsAlarm", "warning", "subscriber_db" };
                    insertCount = insertCount + myDataFactory.Save("replication_SubscriptionStatus", setSubscriptionStatus);  //插入的表记录行数

                    string alarmRatio=alarmCount.ToString()+"/"+tbSyncStatusCount.ToString();
                    if(alarmCount>0){
                        tzAlarm=true;
                    }
                    setTzSubscriptionStatus.Rows.Add(System.Guid.NewGuid().ToString(), nowTime, serverId, _monitorCycle, alarmRatio,repiredCount, tzAlarm, true);
                }
                else {
                    //写入引领表一条记录
                    setTzSubscriptionStatus.Rows.Add(System.Guid.NewGuid(), nowTime, serverId, _monitorCycle, "0/0", repiredCount,tzAlarm, false);            
                }
                ISqlServerDataFactory tzDataFactory = new SqlServerDataFactory(remoteConnectionString);
                string[] tzcolumns = { "MonitorId", "vDate", "serverId", "MonitorCycle", "AlarmRatio", "IsAlarm", "GetSyncStatus"};
                tzDataFactory.Save("tz_Replication", setTzSubscriptionStatus);  //插入的表记录行数

                // return mResult;
                Thread.Sleep(_monitorCycle);
            }
        }
        /// <summary>
        /// 获取本地Ip
        /// </summary>
        /// <returns></returns>
        private static string GetAddressIP()
        {
            ///获取本地的IP地址
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            return AddressIP;
        }
        private string[] InstanceConnectionString()
        {
            string[] myReturn=new string[2];
            string InstanceConnectionString = "";
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select [server_name],server_id from [IndustryEnergy_SH].[dbo].[linkServer]
                                where [server_name] in (select @@SERVERNAME) ";
            DataTable myInstanceName = dataFactory.Query(mySql);
            if (myInstanceName != null && myInstanceName.Rows.Count > 0)
            {
                string mInstanceName = myInstanceName.Rows[0][0].ToString().Trim();
                //Data Source=hengtech01.imwork.net,10885;Initial Catalog=NXJC;User Id=ReplicationUser;Password=songdachuan1990@#
                string[] strs = connectionString.Split(';');
                InstanceConnectionString = @"Data Source=" + mInstanceName;
                for (int i = 1; i < strs.Length; i++)
                {
                    InstanceConnectionString = InstanceConnectionString+";" + strs[i];
                }
            }
            myReturn[0] = InstanceConnectionString;
            myReturn[1] = myInstanceName.Rows[0][1].ToString();
            return myReturn;
        }
        private static DataTable tzSubscriptionStatusTableStructure() 
        {
            DataTable tableStructure = new DataTable();
            tableStructure.Columns.Add("MonitorId", typeof(System.Guid));
            tableStructure.Columns.Add("vDate", typeof(DateTime));
            tableStructure.Columns.Add("serverId", typeof(int));
            tableStructure.Columns.Add("MonitorCycle", typeof(int));
            tableStructure.Columns.Add("AlarmRatio", typeof(string));
            tableStructure.Columns.Add("repired", typeof(int));
            tableStructure.Columns.Add("IsAlarm", typeof(bool));
            tableStructure.Columns.Add("GetSyncStatus", typeof(bool));
            return tableStructure;
        }
        private static DataTable SubscriptionStatusTableStructure()
        {
            DataTable tableStructure = new DataTable();
            tableStructure.Columns.Add("StatusItemId", typeof(string));
            tableStructure.Columns.Add("vDate", typeof(DateTime));
            tableStructure.Columns.Add("serverId", typeof(int));
            tableStructure.Columns.Add("publication", typeof(string));
            tableStructure.Columns.Add("status", typeof(int));
            tableStructure.Columns.Add("monitorranking", typeof(int));
            tableStructure.Columns.Add("MaintenanceResult", typeof(string));
            tableStructure.Columns.Add("IsAlarm", typeof(bool));
            tableStructure.Columns.Add("warning", typeof(int));
            tableStructure.Columns.Add("subscriber_db", typeof(string));
            return tableStructure;
        }
        private static DataTable HelpSubscription()
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString.Replace("NXJC", "distribution"));
            DataTable resultTable = new DataTable();
            string mySql = @"sp_replmonitorhelpsubscription  @publisher =null,
                             @publisher_db=null,
                             @publication=null,
                             @publication_type=0,
                             @mode=0,
                             @exclude_anonymous=0";
            resultTable = dataFactory.Query(mySql);
            return resultTable;
        }
    }
}
