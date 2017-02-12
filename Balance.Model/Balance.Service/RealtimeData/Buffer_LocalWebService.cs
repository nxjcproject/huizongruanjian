using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Balance.Service.RealtimeData
{
    public class LocalDigitalDataGroup
    {
        private ServiceReference_LocalRealtimeData.ArrayOfString _LocalDigitalTagName;
        private ServiceReference_LocalRealtimeData.ArrayOfBoolean _LocalDigitalTagValue;
        public LocalDigitalDataGroup()
        {
            _LocalDigitalTagName = new ServiceReference_LocalRealtimeData.ArrayOfString();
            _LocalDigitalTagValue = new ServiceReference_LocalRealtimeData.ArrayOfBoolean();
        }
        public ServiceReference_LocalRealtimeData.ArrayOfString LocalDigitalTagName
        {
            get
            {
                return _LocalDigitalTagName;
            }
            set
            {
                _LocalDigitalTagName = value;
            }
        }
        public ServiceReference_LocalRealtimeData.ArrayOfBoolean LocalDigitalTagValue
        {
            get
            {
                return _LocalDigitalTagValue;
            }
            set
            {
                _LocalDigitalTagValue = value;
            }
        }
    }
    public class LocalAnalogDataGroup
    {
        private ServiceReference_LocalRealtimeData.ArrayOfString _LocalAnalogTagName;
        private ServiceReference_LocalRealtimeData.ArrayOfDecimal _LocalAnalogTagValue;
        public LocalAnalogDataGroup()
        {
            _LocalAnalogTagName = new ServiceReference_LocalRealtimeData.ArrayOfString();
            _LocalAnalogTagValue = new ServiceReference_LocalRealtimeData.ArrayOfDecimal();
        }
        public ServiceReference_LocalRealtimeData.ArrayOfString LocalAnalogTagName
        {
            get
            {
                return _LocalAnalogTagName;
            }
            set
            {
                _LocalAnalogTagName = value;
            }
        }
        public ServiceReference_LocalRealtimeData.ArrayOfDecimal LocalAnalogTagValue
        {
            get
            {
                return _LocalAnalogTagValue;
            }
            set
            {
                _LocalAnalogTagValue = value;
            }
        }
    }
    /// <summary>
    /// ///////////////////////////增加字符串Web服务///////////////////////
    /// </summary>
    public class LocalStringDataGroup
    {
        private ServiceReference_LocalRealtimeData.ArrayOfString _LocalStringTagName;
        private ServiceReference_LocalRealtimeData.ArrayOfString _LocalStringTagValue;
        public LocalStringDataGroup()
        {
            _LocalStringTagName = new ServiceReference_LocalRealtimeData.ArrayOfString();
            _LocalStringTagValue = new ServiceReference_LocalRealtimeData.ArrayOfString();
        }
        public ServiceReference_LocalRealtimeData.ArrayOfString LocalStringTagName
        {
            get
            {
                return _LocalStringTagName;
            }
            set
            {
                _LocalStringTagName = value;
            }
        }
        public ServiceReference_LocalRealtimeData.ArrayOfString LocalStringTagValue
        {
            get
            {
                return _LocalStringTagValue;
            }
            set
            {
                _LocalStringTagValue = value;
            }
        }
    }

    public class Buffer_LocalWebService
    {
        private const int MaxArrayCount = 50;
        private List<LocalDigitalDataGroup> _DigitalDataGroup;
        private List<LocalAnalogDataGroup> _AnalogDataGroup;
        private List<LocalStringDataGroup> _StringDataGroup;
        public Buffer_LocalWebService()
        {
            _DigitalDataGroup = new List<LocalDigitalDataGroup>();
            _AnalogDataGroup = new List<LocalAnalogDataGroup>();
            _StringDataGroup = new List<LocalStringDataGroup>();
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

        public LocalDigitalDataGroup PopDigitalDataGroup()
        {
            if (_DigitalDataGroup.Count > 0)
            {
                LocalDigitalDataGroup m_LocalDigitalDataGroupTemp = _DigitalDataGroup[0];
                _DigitalDataGroup.RemoveAt(0);
                return m_LocalDigitalDataGroupTemp;
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
                LocalDigitalDataGroup m_LocalDigitalDataGroup = new LocalDigitalDataGroup();
                m_LocalDigitalDataGroup.LocalDigitalTagName.AddRange(myDigitalTagName);
                m_LocalDigitalDataGroup.LocalDigitalTagValue.AddRange(myDigitalTagValue);
                _DigitalDataGroup.Add(m_LocalDigitalDataGroup);
                return true;
            }
            else
            {
                return false;
            }
        }

        public LocalAnalogDataGroup PopAnalogDataGroup()
        {
            if (_AnalogDataGroup.Count > 0)
            {
                LocalAnalogDataGroup m_LocalAnalogDataGroupTemp = _AnalogDataGroup[0];
                _AnalogDataGroup.RemoveAt(0);
                return m_LocalAnalogDataGroupTemp;
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
                LocalAnalogDataGroup m_LocalAnalogDataGroup = new LocalAnalogDataGroup();
                m_LocalAnalogDataGroup.LocalAnalogTagName.AddRange(myAnalogTagName);
                m_LocalAnalogDataGroup.LocalAnalogTagValue.AddRange(myAnalogTagValue);
                _AnalogDataGroup.Add(m_LocalAnalogDataGroup);
                return true;
            }
            else
            {
                return false;
            }
        }
        ///////////////////增加字符串数据////////////////////
        public LocalStringDataGroup PopStringDataGroup()
        {
            if (_StringDataGroup.Count > 0)
            {
                LocalStringDataGroup m_LocalStringDataGroupTemp = _StringDataGroup[0];
                _StringDataGroup.RemoveAt(0);
                return m_LocalStringDataGroupTemp;
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
                LocalStringDataGroup m_LocalStringDataGroup = new LocalStringDataGroup();
                m_LocalStringDataGroup.LocalStringTagName.AddRange(myStringTagName);
                m_LocalStringDataGroup.LocalStringTagValue.AddRange(myStringTagValue);
                _StringDataGroup.Add(m_LocalStringDataGroup);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
