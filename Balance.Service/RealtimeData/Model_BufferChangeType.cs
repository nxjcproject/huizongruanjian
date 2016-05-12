using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Balance.Service.RealtimeData
{
    public class BufferChange
    {
        private string _BufferType;
        private int _Count;
        public BufferChange()
        {
            _BufferType = "";
            _Count = 0;
        }
        public string BufferType
        {
            get
            {
                return _BufferType;
            }
            set
            {
                _BufferType = value;
            }

        }
        public int Count
        {
            get
            {
                return _Count;
            }
            set
            {
                _Count = value;
            }
        }
    }
}
