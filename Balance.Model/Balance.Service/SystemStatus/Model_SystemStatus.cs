using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Balance.Service.SystemStatus
{
    public class Model_SystemAlarm
    {
        private string _NodeId;
        private string _OrganizationId;
        private string _Ip;
        private string _NodeName;
        private string _NodeType;
        private string _InstanceName;
        private int _AlarmCount;
        private string _StartTime;
        private string _Type;
        private bool _UpdateStatus;
        private string _AlarmDescription;
        public Model_SystemAlarm()
        {
            _NodeId = "";
            _OrganizationId = "";
            _Ip = "";
            _NodeName = "";
            _NodeType = "";
            _InstanceName = "";
            _AlarmCount = 0;
            _StartTime = "";
            Type = "";
            _UpdateStatus = false;
            _AlarmDescription = "";
        }
        public string NodeId
        {
            get
            {
                return _NodeId;
            }
            set
            {
                _NodeId = value;
            }
        }
        public string OrganizationId
        {
            get
            {
                return _OrganizationId;
            }
            set
            {
                _OrganizationId = value;
            }
        }
        public string Ip
        {
            get
            {
                return _Ip;
            }
            set
            {
                _Ip = value;
            }
        }
        public string NodeName
        {
            get
            {
                return _NodeName;
            }
            set
            {
                _NodeName = value;
            }
        }
        public string NodeType
        {
            get
            {
                return _NodeType;
            }
            set
            {
                _NodeType = value;
            }
        }
        public string InstanceName
        {
            get
            {
                return _InstanceName;
            }
            set
            {
                _InstanceName = value;
            }
        }
        public int AlarmCount
        {
            get
            {
                return _AlarmCount;
            }
            set
            {
                _AlarmCount = value;
            }
        }
        public string StartTime
        {
            get
            {
                return _StartTime;
            }
            set
            {
                _StartTime = value;
            }
        }
        public string Type
        {
            get
            {
                return _Type;
            }
            set
            {
                _Type = value;
            }
        }
        public bool UpdateStatus
        {
            get
            {
                return _UpdateStatus;
            }
            set
            {
                _UpdateStatus = value;
            }
        }
        public string AlarmDescription
        {
            get
            {
                return _AlarmDescription;
            }
            set
            {
                _AlarmDescription = value;
            }
        }
    }
}
