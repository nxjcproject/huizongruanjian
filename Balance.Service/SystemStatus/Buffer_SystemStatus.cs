using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Balance.Service.SystemStatus
{
    public class Buffer_SystemAlarm
    {
        private static int _MaxAlarmCount;
        private static Dictionary<string, Model_SystemAlarm> _SystemAlarmBuffer;
        public static void InitSystemAlarm()
        {
            _SystemAlarmBuffer = new Dictionary<string,Model_SystemAlarm>();
            _MaxAlarmCount = 4;
        }
        public static void SetAlarm(string myKeyId, Model_SystemAlarm mySystemAlarm)
        {
            if (_SystemAlarmBuffer.ContainsKey(myKeyId))
            {
                int m_CurrentAlarmCount = _SystemAlarmBuffer[myKeyId].AlarmCount;
                if (m_CurrentAlarmCount >= _MaxAlarmCount)
                {
                    _SystemAlarmBuffer[myKeyId].AlarmCount = _MaxAlarmCount;
                    _SystemAlarmBuffer[myKeyId].UpdateStatus = true;
                }
                else
                {
                    _SystemAlarmBuffer[myKeyId].AlarmCount = _SystemAlarmBuffer[myKeyId].AlarmCount + 1;
                    _SystemAlarmBuffer[myKeyId].UpdateStatus = true;
                }
            }
            else
            {
                _SystemAlarmBuffer.Add(myKeyId, mySystemAlarm);
                _SystemAlarmBuffer[myKeyId].AlarmCount = 1;
                _SystemAlarmBuffer[myKeyId].UpdateStatus = true;
            }
        }
        public static void RefreshUpdateStatus()
        {
            if (_SystemAlarmBuffer != null)
            {
                string[] m_SystemAlarmKeysArray = _SystemAlarmBuffer.Keys.ToArray();
                if (m_SystemAlarmKeysArray != null)
                {
                    for (int i = 0; i < m_SystemAlarmKeysArray.Length; i++)
                    {
                        if (_SystemAlarmBuffer[m_SystemAlarmKeysArray[i]].UpdateStatus == false)
                        {
                            _SystemAlarmBuffer[m_SystemAlarmKeysArray[i]].AlarmCount = _SystemAlarmBuffer[m_SystemAlarmKeysArray[i]].AlarmCount - 1;
                            if (_SystemAlarmBuffer[m_SystemAlarmKeysArray[i]].AlarmCount <= 0)
                            {
                                _SystemAlarmBuffer.Remove(m_SystemAlarmKeysArray[i]);
                            }
                        }
                        else
                        {
                            _SystemAlarmBuffer[m_SystemAlarmKeysArray[i]].UpdateStatus = false;
                        }
                    }
                }
            }
        }
        public static Dictionary<string, Model_SystemAlarm> SystemAlarmBuffer
        {
            get
            {
                return _SystemAlarmBuffer;
            }
            set
            {
                _SystemAlarmBuffer = value;
            }
        }
        public static int MaxAlarmCount
        {
            get
            {
                return _MaxAlarmCount;
            }
            set
            {
                _MaxAlarmCount = value;
            }
        }
    }
}
