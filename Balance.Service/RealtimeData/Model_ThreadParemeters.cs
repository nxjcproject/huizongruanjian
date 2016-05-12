using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Balance.Service.RealtimeData
{
    public class Model_UpdateWebServiceThreadParemeters
    {
        private int _DataGroupIndex;
        private string _OrganizationId;
        public Model_UpdateWebServiceThreadParemeters()
        {
            _DataGroupIndex = 0;
            _OrganizationId = "";
        }
        public int DataGroupIndex
        {
            get
            {
                return _DataGroupIndex;
            }
            set
            {
                _DataGroupIndex = value;
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
    }
}
