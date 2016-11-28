using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using SqlServerDataAdapter;
using Balance.Infrastructure.BasicDate;
using Balance.Infrastructure.Configuration;
namespace Balance.Service.SmsSend
{
    public class Function_SmsSender
    {
        private const string SmsMessageTemplate = "能源管理系统报警：{0}";
        private ISqlServerDataFactory _dataFactory;
        private int MaxSendTimesPerMessage;
        private int MaxSendTimesPerNumber;
        private int MaxValidTime;    //有效时间按分钟算
        private int MaxSmsWordLength;   //发送字符长度
        private string EnterpriseCode;
        private string SmsUserId;
        private string SmsPassword;

        public Function_SmsSender()
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            _dataFactory = new SqlServerDataFactory(connectionString);

            InitData();
        }
        private void InitData()
        {
            string m_Sql = @"SELECT TOP (1) A.SmsItemId, 
                                A.SmsName, 
                                A.InterfaceAddress, 
                                A.InterfacePort, 
                                A.UserCode, 
                                A.UserId, 
                                A.Password, 
                                A.SmsTemplate,            
                                A.MaxNumbersInBacth, 
                                A.MaxSmsPerNumberOnDay, 
                                A.MaxSendTimesPerSms, 
                                A.MaxSmsWordLength,
                                A.InvalidTime, 
                                A.Remark
                                FROM terminal_SmsConfig A
                                where A.Enabled = 1";
            try
            {
                DataTable m_SmsConfigTable = _dataFactory.Query(m_Sql);
                if (m_SmsConfigTable != null && m_SmsConfigTable.Rows.Count > 0)
                {
                    MaxSendTimesPerMessage = (int)m_SmsConfigTable.Rows[0]["MaxSendTimesPerSms"];
                    MaxSendTimesPerNumber = (int)m_SmsConfigTable.Rows[0]["MaxSmsPerNumberOnDay"];
                    MaxValidTime = (int)m_SmsConfigTable.Rows[0]["InvalidTime"];    //有效时间按分钟算
                    MaxSmsWordLength = (int)m_SmsConfigTable.Rows[0]["MaxSmsWordLength"];    //短信内容最大长度
                    EnterpriseCode = m_SmsConfigTable.Rows[0]["UserCode"].ToString();
                    SmsUserId = m_SmsConfigTable.Rows[0]["UserId"].ToString();
                    SmsPassword = Function_SystemSecurity.Decrypt3DES(m_SmsConfigTable.Rows[0]["Password"].ToString());
                }
                else
                {
                    MaxSendTimesPerMessage = 0;
                    MaxSendTimesPerNumber = 0;
                    MaxValidTime = 0;    //有效时间按分钟算
                    MaxSmsWordLength = 50;    //短信内容最大长度
                    EnterpriseCode = "";
                    SmsUserId = "";
                    SmsPassword = "";
                }
            }
            catch
            {
                MaxSendTimesPerMessage = 0;
                MaxSendTimesPerNumber = 0;
                MaxValidTime = 0;    //有效时间按分钟算
                MaxSmsWordLength = 50;    //短信内容最大长度
                EnterpriseCode = "";
                SmsUserId = "";
                SmsPassword = "";
            }
        }
        public void SendSms()
        {
            DataTable m_UnsendMessageTable = GetUnsendMessageTable();
            Dictionary<string, string> m_SendMessageByPhoneNumber = MerginSamePhoneNumber(ref m_UnsendMessageTable);
            CheckSendCountPerPhoneNumber(ref m_SendMessageByPhoneNumber, ref m_UnsendMessageTable);   //检查某一个电话号码是否超过发送次数
            SendSms(m_SendMessageByPhoneNumber, ref m_UnsendMessageTable);
            SetMessageStatus(m_UnsendMessageTable);          //短信息状态写回数据库
        }
        private DataTable GetUnsendMessageTable()
        {
            string m_Sql = @"SELECT A.SendItemId
                              ,A.SenderKeyId
                              ,A.SenderType
                              ,A.GroupKey1
                              ,A.GroupKey2
                              ,A.GroupKey3
                              ,A.PhoneNumber
                              ,A.SendCount
                              ,A.CreateTime
                              ,A.OrderSendTime
                              ,A.AlarmText
                              ,A.State
                              ,A.SendResult
                              FROM terminal_SmsSendInfo A
                              where A.State = 0
                              and A.OrderSendTime <= '{0}'
                              order by A.PhoneNumber, A.GroupKey1, A.GroupKey2, A.GroupKey3";
            m_Sql = string.Format(m_Sql, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
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
        private Dictionary<string, string> MerginSamePhoneNumber(ref DataTable myUnsendMessageTable)
        {
            Dictionary<string, string> m_SendMessage = new Dictionary<string, string>();
            if (myUnsendMessageTable != null)
            {
                string m_GroupKey1 = "";
                string m_GroupKey2 = "";
                string m_GroupKey3 = "";
                for (int i = 0; i < myUnsendMessageTable.Rows.Count; i++)
                {
                    string m_PhoneNumber = myUnsendMessageTable.Rows[i]["PhoneNumber"] != DBNull.Value ? myUnsendMessageTable.Rows[i]["PhoneNumber"].ToString() : "";
                    if (m_PhoneNumber == "" || m_PhoneNumber.Length != 11)  //如果电话号码不合法设置为4
                    {
                        //标志4
                        myUnsendMessageTable.Rows[i]["State"] = 4;
                    }
                    else if ((int)myUnsendMessageTable.Rows[i]["SendCount"] >= MaxSendTimesPerMessage)  //该信息已经发送了多次仍未发出
                    {
                        //标志1
                        myUnsendMessageTable.Rows[i]["State"] = 1;
                    }
                    else if (DateTime.Now.AddMinutes(-MaxValidTime) > (DateTime)myUnsendMessageTable.Rows[i]["OrderSendTime"])   //该记录是否超过发送的时间期限
                    {
                        //标志2
                        myUnsendMessageTable.Rows[i]["State"] = 2;
                    }
                    else
                    {
                        myUnsendMessageTable.Rows[i]["SendCount"] = (int)myUnsendMessageTable.Rows[i]["SendCount"] + 1;                         //Count增加1
                        
                        
                        if (m_SendMessage.ContainsKey(m_PhoneNumber))
                        {
                            string m_GroupKeyTemp1 = myUnsendMessageTable.Rows[i]["GroupKey1"] != DBNull.Value ? "##" + myUnsendMessageTable.Rows[i]["GroupKey1"].ToString() + "##\n" : "";
                            string m_GroupKeyTemp2 = myUnsendMessageTable.Rows[i]["GroupKey2"] != DBNull.Value ? "[" + myUnsendMessageTable.Rows[i]["GroupKey2"].ToString() + "]\n" : "";
                            string m_GroupKeyTemp3 = myUnsendMessageTable.Rows[i]["GroupKey3"] != DBNull.Value ? "<" + myUnsendMessageTable.Rows[i]["GroupKey3"].ToString() + ">\n" : "";

                            string m_MessageStringTemp = "";
                            if (m_GroupKey1 != m_GroupKeyTemp1)
                            {
                                m_GroupKey1 = m_GroupKeyTemp1;
                                m_GroupKey2 = m_GroupKeyTemp2;
                                m_GroupKey3 = m_GroupKeyTemp3;
                                m_MessageStringTemp = "\n" + m_GroupKey1 + m_GroupKey2 + m_GroupKey3 + myUnsendMessageTable.Rows[i]["AlarmText"].ToString();
                            }
                            else if (m_GroupKey2 != m_GroupKeyTemp2)
                            {
                                m_GroupKey2 = m_GroupKeyTemp2;
                                m_GroupKey3 = m_GroupKeyTemp3;
                                m_MessageStringTemp = "\n" + m_GroupKey2 + m_GroupKey3 + myUnsendMessageTable.Rows[i]["AlarmText"].ToString();
                            }
                            else if (m_GroupKey3 != m_GroupKeyTemp3)
                            {
                                m_GroupKey3 = m_GroupKeyTemp3;
                                m_MessageStringTemp = "\n" + m_GroupKey3 + myUnsendMessageTable.Rows[i]["AlarmText"].ToString();
                            }
                            else
                            {
                                m_MessageStringTemp = myUnsendMessageTable.Rows[i]["AlarmText"].ToString();
                            }
                            m_SendMessage[m_PhoneNumber] = m_SendMessage[m_PhoneNumber] + ";" + m_MessageStringTemp;
                        }
                        else
                        {
                            m_GroupKey1 = myUnsendMessageTable.Rows[i]["GroupKey1"] != DBNull.Value ? "##" + myUnsendMessageTable.Rows[i]["GroupKey1"].ToString() + "##\n" : "";
                            m_GroupKey2 = myUnsendMessageTable.Rows[i]["GroupKey2"] != DBNull.Value ? "[" + myUnsendMessageTable.Rows[i]["GroupKey2"].ToString() + "]\n" : "";
                            m_GroupKey3 = myUnsendMessageTable.Rows[i]["GroupKey3"] != DBNull.Value ? "<" + myUnsendMessageTable.Rows[i]["GroupKey3"].ToString() + ">\n" : "";
                            string m_MessageStringTemp = m_GroupKey1 + m_GroupKey2 + m_GroupKey3 + myUnsendMessageTable.Rows[i]["AlarmText"].ToString();
                            m_SendMessage.Add(m_PhoneNumber, m_MessageStringTemp);
                        }
                    }
                }
            }
            return m_SendMessage;
        }
        private void CheckSendCountPerPhoneNumber(ref Dictionary<string, string> mySendMessageByPhoneNumber, ref DataTable myUnsendMessageTable)
        {
            if (mySendMessageByPhoneNumber.Count > 0)
            {
                string m_Sql = @"SELECT A.PhoneNumber, count(A.PhoneNumber) as Count                        
                              FROM terminal_SmsSendInfo A
                              where A.State = 99
                              and A.OrderSendTime >= '{0} 00:00:00'
                              and A.OrderSendTime < '{1} 00:00:00'
                              and A.PhoneNumber in ({2})
                              group by A.PhoneNumber 
                              order by A.PhoneNumber";
                string m_PhoneNumberList = "";
                foreach (string myKey in mySendMessageByPhoneNumber.Keys)
                {
                    m_PhoneNumberList = ",'" + myKey + "'";
                }
                if (m_PhoneNumberList.Length > 0)
                {
                    m_PhoneNumberList = m_PhoneNumberList.Substring(1);
                }
                else
                {
                    m_PhoneNumberList = "''";
                }
                m_Sql = string.Format(m_Sql, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"), m_PhoneNumberList);
                try
                {
                    DataTable m_SmsSendInfoTable = _dataFactory.Query(m_Sql);
                    if (m_SmsSendInfoTable != null)
                    {
                        for (int i = 0; i < m_SmsSendInfoTable.Rows.Count; i++)
                        {
                            if ((int)m_SmsSendInfoTable.Rows[i]["Count"] > MaxSendTimesPerNumber)
                            {
                                //标志3
                                myUnsendMessageTable.Rows[i]["State"] = 3;
                                mySendMessageByPhoneNumber.Remove(m_SmsSendInfoTable.Rows[i]["PhoneNumber"].ToString());
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }
        private void SendSms(Dictionary<string, string> mySendMessageByPhoneNumber, ref DataTable myUnsendMessageTable)
        {
            string[] m_InParameters = new string[11];
            m_InParameters[0] = EnterpriseCode;            //企业编号
            m_InParameters[1] = SmsUserId;            //用户名
            m_InParameters[2] = SmsPassword;            //用户密码
            m_InParameters[3] = "";             //短信内容, 最大402个字或字符
            m_InParameters[4] = "";            //手机号码(多个号码用”,”分隔)，最多1000个号码
            m_InParameters[5] = "";            //流水号，20位数字，每个请求流水号要求唯一（规则自定义,建议时间格式精确到毫秒）
            m_InParameters[6] = "";            //预约发送时间，格式:yyyyMMddHHmmss,如‘20090901010101’， 立即发送请填空（预约时间要写当前时间5分钟之后的时间，若预约时间少于5分钟，则为立即发送。）
            m_InParameters[7] = "1";            //提交时检测方式,1 --- 提交号码中有效的号码仍正常发出短信，无效的号码在返回参数faillist中列出;不为1 或该参数不存在 --- 提交号码中只要有无效的号码，那么所有的号码都不发出短信，所有的号码在返回参数faillist中列出
            m_InParameters[8] = "";            //保留（空值）
            m_InParameters[9] = "";            //接入号扩展号（默认不填，扩展号为数字，扩展位数由当前所配的接入号长度决定，整个接入号最长20位）
            m_InParameters[10] = "";            //保留（空值）
            try
            {
                if (mySendMessageByPhoneNumber.Count > 0)
                {
                    Balance.Service.ServiceReference_SmsSender.SmsPortTypeClient m_Client = new Balance.Service.ServiceReference_SmsSender.SmsPortTypeClient();
                    try
                    {
                        m_Client.Open();
                        foreach (string myKey in mySendMessageByPhoneNumber.Keys)
                        {
                            string m_SendMessage = mySendMessageByPhoneNumber[myKey].Length > MaxSmsWordLength ? mySendMessageByPhoneNumber[myKey].Substring(0, MaxSmsWordLength) : mySendMessageByPhoneNumber[myKey];
                            m_InParameters[3] = string.Format(SmsMessageTemplate, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n" + m_SendMessage);
                            m_InParameters[4] = myKey;
                            m_InParameters[5] = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            string m_Result = m_Client.Sms(m_InParameters[0], m_InParameters[1], m_InParameters[2], m_InParameters[3], m_InParameters[4], m_InParameters[5]
                                              , m_InParameters[6], m_InParameters[7], m_InParameters[8], m_InParameters[9], m_InParameters[10]);
                            SetMessageStatus(myKey, m_Result, ref myUnsendMessageTable);   //返回结果后设置短信发送状态
                        }
                        m_Client.Close();
                    }
                    catch
                    {
                        if (m_Client.State == System.ServiceModel.CommunicationState.Opened || m_Client.State == System.ServiceModel.CommunicationState.Faulted)
                        {
                            m_Client.Close();
                        }
                    }
                    finally
                    {
                        if (m_Client.State == System.ServiceModel.CommunicationState.Opened || m_Client.State == System.ServiceModel.CommunicationState.Faulted)
                        {
                            m_Client.Close();
                        }
                    }
                }
            }
            catch
            {
            }
        }
        private void SetMessageStatus(string myPhoneNumber, string myResultString, ref DataTable myUnsendMessageTable)
        {
            try
            {
                string[] m_Result = myResultString.Split('&');
                for (int i = 0; i < m_Result.Length; i++)
                {
                    string[] m_ResultWord = m_Result[i].Split('=');
                    if (m_ResultWord.Length > 1)
                    {
                        if (m_ResultWord[0] == "result" && m_ResultWord[1] == "0")
                        {
                            for (int j = 0; j < myUnsendMessageTable.Rows.Count; j++)
                            {
                                if (myUnsendMessageTable.Rows[j]["PhoneNumber"].ToString() == myPhoneNumber)
                                {
                                    string m_SendResult = myUnsendMessageTable.Rows[j]["SendResult"] != DBNull.Value ? myUnsendMessageTable.Rows[j]["SendResult"].ToString() : "";
                                    myUnsendMessageTable.Rows[j]["State"] = 99;
                                    myUnsendMessageTable.Rows[j]["SendResult"] = m_SendResult + m_ResultWord[1] + ";";
                                }
                            }
                        }
                        else if (m_ResultWord[0] == "result")       //如果是不成功,则记录状态字
                        {
                            for (int j = 0; j < myUnsendMessageTable.Rows.Count; j++)
                            {
                                if (myUnsendMessageTable.Rows[j]["PhoneNumber"].ToString() == myPhoneNumber)
                                {
                                    string m_SendResult = myUnsendMessageTable.Rows[j]["SendResult"] != DBNull.Value ? myUnsendMessageTable.Rows[j]["SendResult"].ToString() : "";
                                    myUnsendMessageTable.Rows[j]["SendResult"] = m_SendResult + m_ResultWord[1] + ";";
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }
        private void SetMessageStatus(DataTable myUnsendMessageTable)
        {
            try
            {
                _dataFactory.Update("terminal_SmsSendInfo", myUnsendMessageTable, new string[] { "SendItemId" });
            }
            catch
            {
            }
        }
    }
}
