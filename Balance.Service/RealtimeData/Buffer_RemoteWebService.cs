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
    public class Buffer_RemoteWebService
    {
        private const int MaxArrayCount = 50;
        private List<RemoteDigitalDataGroup> _DigitalDataGroup;
        private List<RemoteAnalogDataGroup> _AnalogDataGroup;
        public Buffer_RemoteWebService()
        {
            _DigitalDataGroup = new List<RemoteDigitalDataGroup>();
            _AnalogDataGroup = new List<RemoteAnalogDataGroup>();
        }
        public bool CanWrite
        {
            get
            {
                if (_DigitalDataGroup.Count <= MaxArrayCount && _AnalogDataGroup.Count <= MaxArrayCount)
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
        public int AnalogCount
        {
            get
            {
                return _AnalogDataGroup.Count;
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
    }
}
