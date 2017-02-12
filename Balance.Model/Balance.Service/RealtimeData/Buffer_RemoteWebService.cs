using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Balance.Service.RealtimeData
{
    public class RemoteDigitalDataGroup
    {
        private ServiceReference_RemoteRealtimeData.ArrayOfString _RemoteDigitalTagName;
        private ServiceReference_RemoteRealtimeData.ArrayOfBoolean _RemoteDigitalTagValue;
        public RemoteDigitalDataGroup()
        {
            _RemoteDigitalTagName = new ServiceReference_RemoteRealtimeData.ArrayOfString();
            _RemoteDigitalTagValue = new ServiceReference_RemoteRealtimeData.ArrayOfBoolean();
        }
        public ServiceReference_RemoteRealtimeData.ArrayOfString RemoteDigitalTagName
        {
            get
            {
                return _RemoteDigitalTagName;
            }
            set
            {
                _RemoteDigitalTagName = value;
            }
        }
        public ServiceReference_RemoteRealtimeData.ArrayOfBoolean RemoteDigitalTagValue
        {
            get
            {
                return _RemoteDigitalTagValue;
            }
            set
            {
                _RemoteDigitalTagValue = value;
            }
        }
    }
    public class RemoteAnalogDataGroup
    {
        private ServiceReference_RemoteRealtimeData.ArrayOfString _RemoteAnalogTagName;
        private ServiceReference_RemoteRealtimeData.ArrayOfDecimal _RemoteAnalogTagValue;
        public RemoteAnalogDataGroup()
        {
            _RemoteAnalogTagName = new ServiceReference_RemoteRealtimeData.ArrayOfString();
            _RemoteAnalogTagValue = new ServiceReference_RemoteRealtimeData.ArrayOfDecimal();
        }
        public ServiceReference_RemoteRealtimeData.ArrayOfString RemoteAnalogTagName
        {
            get
            {
                return _RemoteAnalogTagName;
            }
            set
            {
                _RemoteAnalogTagName = value;
            }
        }
        public ServiceReference_RemoteRealtimeData.ArrayOfDecimal RemoteAnalogTagValue
        {
            get
            {
                return _RemoteAnalogTagValue;
            }
            set
            {
                _RemoteAnalogTagValue = value;
            }
        }
    }
    /// <summary>
    /// ///////////////////////////增加字符串Web服务///////////////////////
    /// </summary>
    public class RemoteStringDataGroup
    {
        private ServiceReference_RemoteRealtimeData.ArrayOfString _RemoteStringTagName;
        private ServiceReference_RemoteRealtimeData.ArrayOfString _RemoteStringTagValue;
        public RemoteStringDataGroup()
        {
            _RemoteStringTagName = new ServiceReference_RemoteRealtimeData.ArrayOfString();
            _RemoteStringTagValue = new ServiceReference_RemoteRealtimeData.ArrayOfString();
        }
        public ServiceReference_RemoteRealtimeData.ArrayOfString RemoteStringTagName
        {
            get
            {
                return _RemoteStringTagName;
            }
            set
            {
                _RemoteStringTagName = value;
            }
        }
        public ServiceReference_RemoteRealtimeData.ArrayOfString RemoteStringTagValue
        {
            get
            {
                return _RemoteStringTagValue;
            }
            set
            {
                _RemoteStringTagValue = value;
            }
        }
    }

    public class Buffer_RemoteWebService
    {
        private const int MaxArrayCount = 50;
        private List<RemoteDigitalDataGroup> _DigitalDataGroup;
        private List<RemoteAnalogDataGroup> _AnalogDataGroup;
        private List<RemoteStringDataGroup> _StringDataGroup;
        public Buffer_RemoteWebService()
        {
            _DigitalDataGroup = new List<RemoteDigitalDataGroup>();
            _AnalogDataGroup = new List<RemoteAnalogDataGroup>();
            _StringDataGroup = new List<RemoteStringDataGroup>();
        }
        public bool CanWrite
        {
            get
            {
                if (_DigitalDataGroup.Count <= MaxArrayCount && _AnalogDataGroup.Count <= MaxArrayCount && _StringDataGroup.Count <= MaxArrayCount)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
      
        public RemoteDigitalDataGroup PopDigitalDataGroup()
        {
            if (_DigitalDataGroup.Count > 0)
            {
                RemoteDigitalDataGroup m_RemoteDigitalDataGroupTemp = _DigitalDataGroup[0];
                _DigitalDataGroup.RemoveAt(0);
                return m_RemoteDigitalDataGroupTemp;
            }
            else
            {
                return null;
            }
        }
        public int DigitalCount
        {
            get
            {
                return _DigitalDataGroup.Count;
            }
        }
        public bool SetDigitalDataGroup(List<string> myDigitalTagName, List<bool> myDigitalTagValue)
        {
            if (_DigitalDataGroup.Count <= MaxArrayCount)
            {
                RemoteDigitalDataGroup m_RemoteDigitalDataGroup = new RemoteDigitalDataGroup();
                m_RemoteDigitalDataGroup.RemoteDigitalTagName.AddRange(myDigitalTagName);
                m_RemoteDigitalDataGroup.RemoteDigitalTagValue.AddRange(myDigitalTagValue);
                _DigitalDataGroup.Add(m_RemoteDigitalDataGroup);
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public RemoteAnalogDataGroup PopAnalogDataGroup()
        {
            if (_AnalogDataGroup.Count > 0)
            {
                RemoteAnalogDataGroup m_RemoteAnalogDataGroupTemp = _AnalogDataGroup[0];
                _AnalogDataGroup.RemoveAt(0);
                return m_RemoteAnalogDataGroupTemp;
            }
            else
            {
                return null;
            }
        }
        public int AnalogCount
        {
            get
            {
                return _AnalogDataGroup.Count;
            }
        }
        public bool SetAnalogDataGroup(List<string> myAnalogTagName, List<decimal> myAnalogTagValue)
        {
            if (_AnalogDataGroup.Count <= MaxArrayCount)
            {
                RemoteAnalogDataGroup m_RemoteAnalogDataGroup = new RemoteAnalogDataGroup();
                m_RemoteAnalogDataGroup.RemoteAnalogTagName.AddRange(myAnalogTagName);
                m_RemoteAnalogDataGroup.RemoteAnalogTagValue.AddRange(myAnalogTagValue);
                _AnalogDataGroup.Add(m_RemoteAnalogDataGroup);
                return true;
            }
            else
            {
                return false;
            }
        }
        ///////////////////增加字符串数据////////////////////
        public RemoteStringDataGroup PopStringDataGroup()
        {
            if (_StringDataGroup.Count > 0)
            {
                RemoteStringDataGroup m_RemoteStringDataGroupTemp = _StringDataGroup[0];
                _StringDataGroup.RemoveAt(0);
                return m_RemoteStringDataGroupTemp;
            }
            else
            {
                return null;
            }
        }
        public int StringCount
        {
            get
            {
                return _StringDataGroup.Count;
            }
        }
        public bool SetStringDataGroup(List<string> myStringTagName, List<string> myStringTagValue)
        {
            if (_StringDataGroup.Count <= MaxArrayCount)
            {
                RemoteStringDataGroup m_RemoteStringDataGroup = new RemoteStringDataGroup();
                m_RemoteStringDataGroup.RemoteStringTagName.AddRange(myStringTagName);
                m_RemoteStringDataGroup.RemoteStringTagValue.AddRange(myStringTagValue);
                _StringDataGroup.Add(m_RemoteStringDataGroup);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
