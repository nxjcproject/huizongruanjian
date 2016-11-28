using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
namespace Balance.Service.SmsSend
{
    public class Realtime_SmsSend
    {
        private bool _IsClosing;
        public void StartSmsSend()
        {
            Thread m_Thread_SetRealtimeData = new Thread(SmsSend);
            m_Thread_SetRealtimeData.Start();
        }
        private void SmsSend()
        {
            Balance.Service.SmsSend.Function_SmsSender m_SmsSend = new Balance.Service.SmsSend.Function_SmsSender();
            int m_SleepTime = 1000 * 60 * 5;    //5分钟检测一次
            while (IsClosing == false)
            {
                Thread.Sleep(m_SleepTime);
                m_SmsSend.SendSms();   //发送短信
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
