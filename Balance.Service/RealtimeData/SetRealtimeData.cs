using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Configuration;
using System.Threading;
using System.Management;
using SqlServerDataAdapter;
using Balance.Infrastructure.BasicDate;
using Balance.Infrastructure.Configuration;

namespace Balance.Service.RealtimeData
{
    public class SetRealtimeData
    {
        private const string ValidWriteString = "HTKJ134fqewrg?%%da@@";
        //private const int QueryTime = 150;
        //private const int UpdateWebServiceTime = 100;
        private bool _IsClosing;
        private int UpdateInterval;            //更新周期
        private int MinQueryInterval;          //最小查询时间间隔
        private int DBThreadItemsCount;        //查询数据库线程中数据查询个数
        /////////////////实时线程最大查询时间/////////////////////
        private int RealtimeMaxQueryThreadTime;
        private int RealtimeMaxQueryThreadCount;
        private ISqlServerDataFactory _dataFactory;
        private string[] _FactoryOrganizationId;
        private List<Buffer_LocalWebService> _LocalWebServiceBuffer;         //建立本地更新webservice缓冲区
        private List<Buffer_RemoteWebService> _RemoteWebServiceBuffer;       //建立远程更新webservice缓冲区

        //自定义事件
        public delegate void EventHandler(object sender, BufferChange e);
        public event EventHandler BufferChangeHandler;
        protected void onBufferChange(string myType, int myCount)
        {
            BufferChange m_BufferChange = new BufferChange();
            m_BufferChange.BufferType = myType;
            m_BufferChange.Count = myCount;
            BufferChangeHandler(this, m_BufferChange);
        }
        
        public SetRealtimeData()
        {
            _IsClosing = false;
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            _dataFactory = new SqlServerDataFactory(connectionString);
            UpdateInterval = ConfigService.UpdateInterval;
            MinQueryInterval = ConfigService.MinQueryInterval;
            DBThreadItemsCount = ConfigService.DBThreadItemsCount;

            _FactoryOrganizationId = ConfigService.FactoryOrganizationId;

            _LocalWebServiceBuffer = new List<Buffer_LocalWebService>();
            _RemoteWebServiceBuffer = new List<Buffer_RemoteWebService>();

            RealtimeMaxQueryThreadTime = 0;
            RealtimeMaxQueryThreadCount = 0;
        }
        public void SetDataToWebService()
        {
            DataTable m_DataBaseInfoTable = GetDataBaseInfo();                 //获得DataBase名称
            if (m_DataBaseInfoTable != null)
            {
                for (int i = 0; i < m_DataBaseInfoTable.Rows.Count; i++)  //一个DCS开始一个线程,一个DCS下5个一组
                {
                    DataTable m_RealtimeDataTableNameTable = GetRealtimeDataTableName(m_DataBaseInfoTable.Rows[i]["Name"].ToString());
                    if (m_RealtimeDataTableNameTable != null)
                    {
                        string m_DataTableName = "";
                        for (int j = 0; j < m_RealtimeDataTableNameTable.Rows.Count; j++)
                        {
                            if (j % DBThreadItemsCount == 0)
                            {
                                m_DataTableName = m_RealtimeDataTableNameTable.Rows[j]["Name"].ToString();
                            }
                            else if (j % DBThreadItemsCount == DBThreadItemsCount - 1)             //每n个数值一组,一个线程
                            {
                                m_DataTableName = m_DataTableName + "," + m_RealtimeDataTableNameTable.Rows[j]["Name"].ToString();
                                string m_PrefixWord = m_DataBaseInfoTable.Rows[i]["Name"].ToString();
                                m_PrefixWord = m_PrefixWord.Substring(m_PrefixWord.LastIndexOf('_') + 1) + "_";
                                Thread m_Thread_SetRealtimeData = new Thread(new ParameterizedThreadStart(SetData));
                                m_Thread_SetRealtimeData.Start(new string[] {m_DataTableName, m_PrefixWord, i.ToString()});
                            }
                            else if (j == m_RealtimeDataTableNameTable.Rows.Count - 1)   //当最后一个并不能整除5的时候
                            {
                                m_DataTableName = m_DataTableName + "," + m_RealtimeDataTableNameTable.Rows[j]["Name"].ToString();
                                string m_PrefixWord = m_DataBaseInfoTable.Rows[i]["Name"].ToString();
                                m_PrefixWord = m_PrefixWord.Substring(m_PrefixWord.LastIndexOf('_') + 1) + "_";
                                Thread m_Thread_SetRealtimeData = new Thread(new ParameterizedThreadStart(SetData));
                                m_Thread_SetRealtimeData.Start(new string[] { m_DataTableName, m_PrefixWord, i.ToString() });
                            }
                            else
                            {
                                m_DataTableName = m_DataTableName + "," + m_RealtimeDataTableNameTable.Rows[j]["Name"].ToString();
                            }
                        }
                        //////////////////更新到webService,一个DCS一套数据缓冲区//////////////
                        Buffer_LocalWebService m_LocalWebService = new Buffer_LocalWebService();
                        _LocalWebServiceBuffer.Add(m_LocalWebService);
                        Buffer_RemoteWebService m_RemoteWebService = new Buffer_RemoteWebService();
                        _RemoteWebServiceBuffer.Add(m_RemoteWebService);

                        //////////////////更新到webService,一个DCS一套更新线程//////////////
                        Model_UpdateWebServiceThreadParemeters m_LocalThread = new Model_UpdateWebServiceThreadParemeters();
                        m_LocalThread.DataGroupIndex = i;
                        m_LocalThread.OrganizationId = m_DataBaseInfoTable.Rows[i]["OrganizationId"].ToString();
                        Thread m_Thread_UpdateDataToLocal = new Thread(new ParameterizedThreadStart(UpdateLocalData));
                        m_Thread_UpdateDataToLocal.Start(m_LocalThread);

                        Model_UpdateWebServiceThreadParemeters m_RemoveThread = new Model_UpdateWebServiceThreadParemeters();
                        m_RemoveThread.DataGroupIndex = i;
                        m_RemoveThread.OrganizationId = m_DataBaseInfoTable.Rows[i]["OrganizationId"].ToString();
                        Thread m_Thread_UpdateDataToRemote = new Thread(new ParameterizedThreadStart(UpdateRemoteData));
                        m_Thread_UpdateDataToRemote.Start(m_RemoveThread);
                    }
                }
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
        private DataTable GetDataBaseInfo()
        {
            string m_Sql = @"SELECT A.Name, B.OrganizationID as OrganizationId FROM Master..SysDatabases A, system_Organization B, system_Database C
                where B.OrganizationID in ({0})
                and B.DatabaseID = C.DatabaseID
                and A.Name like C.MeterDatabase + '_%'
                order by B.OrganizationID";
            string m_Condition = "''";
                for(int i=0;i< _FactoryOrganizationId.Length;i++)
                {
                    if(i==0)
                    {
                        m_Condition = "'" + _FactoryOrganizationId[i] + "'";
                    }
                    else
                    {
                        m_Condition = m_Condition + ",'" + _FactoryOrganizationId[i] + "'";
                    }
                }
            try
            {
                DataTable m_DataBaseInfoTable = _dataFactory.Query(string.Format(m_Sql, m_Condition));
                return m_DataBaseInfoTable;
            }
            catch
            {
                return null;
            }
        }
        private void UpdateLocalData(object myUpdateParemeters)
        {
            try
            {
                ServiceReference_LocalRealtimeData.RealTimeDataSoapClient m_LocalRealtimeDataService = new ServiceReference_LocalRealtimeData.RealTimeDataSoapClient();

                int m_DataGroupIndex = ((Model_UpdateWebServiceThreadParemeters)myUpdateParemeters).DataGroupIndex;
                string m_OrganizationId = ((Model_UpdateWebServiceThreadParemeters)myUpdateParemeters).OrganizationId;
                Thread.Sleep(2000);
                while (_IsClosing == false)
                {
                    LocalDigitalDataGroup m_LocalDigitalDataGroupTemp = _LocalWebServiceBuffer[m_DataGroupIndex].PopDigitalDataGroup();
                    LocalAnalogDataGroup m_LocalAnalogDataGroupTemp = _LocalWebServiceBuffer[m_DataGroupIndex].PopAnalogDataGroup();

                    if (m_LocalDigitalDataGroupTemp != null)
                    {
                        byte[] m_DigitalTagName = DataCompression.Function_DefaultCompressionArray.CompressString(m_LocalDigitalDataGroupTemp.LocalDigitalTagName.ToArray());
                        byte[] m_DigitalTagValue = DataCompression.Function_DefaultCompressionArray.CompressBoolen(m_LocalDigitalDataGroupTemp.LocalDigitalTagValue.ToArray());
                        try
                        {
                            m_LocalRealtimeDataService.SetDigitalDataCompress(m_OrganizationId, m_DigitalTagName, m_DigitalTagValue, ValidWriteString);
                        }
                        catch
                        {
                        }
                        //监控远程Webservice数据缓冲区
                        int m_Count = 0;
                        for (int i = 0; i < _LocalWebServiceBuffer.Count; i++)
                        {
                            m_Count = m_Count + _LocalWebServiceBuffer[i].DigitalCount;
                        }
                        onBufferChange("LocalDigitalData", m_Count);
                    }
                    if (m_LocalAnalogDataGroupTemp != null)
                    {
                        byte[] m_AnalogTagName = DataCompression.Function_DefaultCompressionArray.CompressString(m_LocalAnalogDataGroupTemp.LocalAnalogTagName.ToArray());
                        byte[] m_AnalogTagValue = DataCompression.Function_DefaultCompressionArray.CompressDecimal(m_LocalAnalogDataGroupTemp.LocalAnalogTagValue.ToArray());
                        try
                        {
                            m_LocalRealtimeDataService.SetAnalogDataCompress(m_OrganizationId, m_AnalogTagName, m_AnalogTagValue, ValidWriteString);
                        }
                        catch
                        {
                        }
                        //监控远程Webservice数据缓冲区
                        int m_Count = 0;
                        for (int i = 0; i < _LocalWebServiceBuffer.Count; i++)
                        {
                            m_Count = m_Count + _LocalWebServiceBuffer[i].AnalogCount;
                        }
                        onBufferChange("LocalAnalogData", m_Count);
                    }
                    Thread.Sleep(MinQueryInterval);
                }
            }
            catch
            {

            }
        }
        private void UpdateRemoteData(object myUpdateParemeters)
        {
            try
            {
                ServiceReference_RemoteRealtimeData.RealTimeDataSoapClient m_RemoteRealtimeDataService = new ServiceReference_RemoteRealtimeData.RealTimeDataSoapClient();

                int m_DataGroupIndex = ((Model_UpdateWebServiceThreadParemeters)myUpdateParemeters).DataGroupIndex;
                string m_OrganizationId = ((Model_UpdateWebServiceThreadParemeters)myUpdateParemeters).OrganizationId;
                Thread.Sleep(2000);
                while (_IsClosing == false)
                {
                    RemoteDigitalDataGroup m_RemoteDigitalDataGroupTemp = _RemoteWebServiceBuffer[m_DataGroupIndex].PopDigitalDataGroup();
                    RemoteAnalogDataGroup m_RemoteAnalogDataGroupTemp = _RemoteWebServiceBuffer[m_DataGroupIndex].PopAnalogDataGroup();
                    if (m_RemoteDigitalDataGroupTemp != null)
                    {
                        byte[] m_DigitalTagName = DataCompression.Function_DefaultCompressionArray.CompressString(m_RemoteDigitalDataGroupTemp.RemoteDigitalTagName.ToArray());
                        byte[] m_DigitalTagValue = DataCompression.Function_DefaultCompressionArray.CompressBoolen(m_RemoteDigitalDataGroupTemp.RemoteDigitalTagValue.ToArray());
                        try
                        {
                            m_RemoteRealtimeDataService.SetDigitalDataCompress(m_OrganizationId, m_DigitalTagName, m_DigitalTagValue, ValidWriteString);
                        }
                        catch
                        {
                        }
                        //监控远程Webservice数据缓冲区
                        int m_Count = 0;
                        for (int i = 0; i < _RemoteWebServiceBuffer.Count; i++)
                        {
                            m_Count = m_Count + _RemoteWebServiceBuffer[i].DigitalCount;
                        }
                        onBufferChange("RemoteDigitalData", m_Count);
                    }
                    if (m_RemoteAnalogDataGroupTemp != null)
                    {
                        byte[] m_AnalogTagName = DataCompression.Function_DefaultCompressionArray.CompressString(m_RemoteAnalogDataGroupTemp.RemoteAnalogTagName.ToArray());
                        byte[] m_AnalogTagValue = DataCompression.Function_DefaultCompressionArray.CompressDecimal(m_RemoteAnalogDataGroupTemp.RemoteAnalogTagValue.ToArray());
                        try
                        {
                            m_RemoteRealtimeDataService.SetAnalogDataCompress(m_OrganizationId, m_AnalogTagName, m_AnalogTagValue, ValidWriteString);
                        }
                        catch
                        {
                        }
                        int m_Count = 0;
                        for (int i = 0; i < _RemoteWebServiceBuffer.Count; i++)
                        {
                            m_Count = m_Count + _RemoteWebServiceBuffer[i].AnalogCount;
                        }
                        onBufferChange("RemoteAnalogData", m_Count);
                    }
                    Thread.Sleep(MinQueryInterval);
                }
            }
            catch
            {

            }
        }


        /// <summary>
        /// 获取数据并保存数据到webservice
        /// </summary>
        /// <param name="myDataBaseName">数据库名</param>
        private void SetData(object myDataTableNames)
        {
            Thread.Sleep(1000);
            string[] m_DataTableNames = (string[])myDataTableNames;
            if (m_DataTableNames != null)
            {
                string[] m_DataTableName = m_DataTableNames[0].Split(',');
                string m_PrefixWord = m_DataTableNames[1];
                int m_DataGroupIndex = Int32.Parse(m_DataTableNames[2]);
                while (_IsClosing == false)
                {
                    DateTime m_ThreadStartTime = DateTime.Now;
                    GetRealTimeDataFromDB(m_DataTableName, m_PrefixWord, m_DataGroupIndex);

                    double m_TotalsQueryTime = (DateTime.Now - m_ThreadStartTime).TotalMilliseconds;
                    int m_SleepTime = UpdateInterval - (int)m_TotalsQueryTime;

                    ///////////////////以下是做监控处理//////////////////////
                    //监控线程查询时间
                    if (RealtimeMaxQueryThreadCount < 2000)
                    {
                        if (m_TotalsQueryTime > RealtimeMaxQueryThreadTime)
                        {
                            RealtimeMaxQueryThreadTime = (int)m_TotalsQueryTime;
                            onBufferChange("QueryThreadTime", RealtimeMaxQueryThreadTime);
                        }
                        RealtimeMaxQueryThreadCount = RealtimeMaxQueryThreadCount + 1;
                    }
                    else          //如果大于等于2000重新计算最大线程查询时间
                    {
                        RealtimeMaxQueryThreadCount = 1;
                        RealtimeMaxQueryThreadTime = (int)m_TotalsQueryTime;
                        onBufferChange("QueryThreadTime", RealtimeMaxQueryThreadTime);
                    }
                    /////////////////////////////////////////////////////


                    if (m_TotalsQueryTime > 0 && m_SleepTime > 0)
                    {
                        Thread.Sleep(m_SleepTime);
                    }               
                }
            }

        }
        private void GetRealTimeDataFromDB(string[] myDataTableName, string myPrefixWord, int myDataGroupIndex)
        {
            List<string> m_DigitalTagName = new List<string>();
            List<bool> m_DigitalTagValue = new List<bool>();
            List<string> m_AnalogTagName = new List<string>();
            List<decimal> m_AnalogTagValue = new List<decimal>();

            //int m_SleepInterval = (UpdateInterval - QueryTime * myDataTableName.Length - UpdateWebServiceTime) / myDataTableName.Length;   //更新时间减去查找时间就是可以自由分配的时间
            if (_LocalWebServiceBuffer[myDataGroupIndex].CanWrite == true || _RemoteWebServiceBuffer[myDataGroupIndex].CanWrite == true)            //只有大于0才执行
            {
                for (int i = 0; i < myDataTableName.Length; i++)
                {
                    DataTable m_RealtimeValueTable = GetRealtimeValueInfo(myDataTableName[i]);
                    if (m_RealtimeValueTable != null && m_RealtimeValueTable.Rows.Count > 0)
                    {
                        for (int j = 0; j < m_RealtimeValueTable.Columns.Count; j++)
                        {
                            //Type mm = m_RealtimeValueTable.Columns[j].GetType();

                            if (m_RealtimeValueTable.Columns[j].DataType == typeof(bool) && m_RealtimeValueTable.Rows[0][j] != DBNull.Value)  //如果是开关量
                            {
                                m_DigitalTagName.Add(myPrefixWord + m_RealtimeValueTable.Columns[j].ColumnName);
                                m_DigitalTagValue.Add((bool)m_RealtimeValueTable.Rows[0][j]);
                            }
                            else if (m_RealtimeValueTable.Columns[j].DataType == typeof(decimal) && m_RealtimeValueTable.Rows[0][j] != DBNull.Value)   //如果是模拟量
                            {
                                m_AnalogTagName.Add(myPrefixWord + m_RealtimeValueTable.Columns[j].ColumnName);
                                m_AnalogTagValue.Add((decimal)m_RealtimeValueTable.Rows[0][j]);
                            }
                        }
                    }
                    Thread.Sleep(MinQueryInterval);
                }
                if (m_DigitalTagName.Count > 0)            //传送开关量
                {
                    _LocalWebServiceBuffer[myDataGroupIndex].SetDigitalDataGroup(m_DigitalTagName, m_DigitalTagValue);
                    _RemoteWebServiceBuffer[myDataGroupIndex].SetDigitalDataGroup(m_DigitalTagName, m_DigitalTagValue);
                    //LocalRealtimeDataService.SetDigitalData(_FactoryOrganizationId, m_LocalDigitalTagName, m_LocalDigitalTagValue, ValidWriteString);
                    //RemoteRealtimeDataService.SetDigitalData(_FactoryOrganizationId, m_RemoteDigitalTagName, m_RemoteDigitalTagValue, ValidWriteString);
                }
                if (m_AnalogTagName.Count > 0)             //传送模拟量
                {
                    _LocalWebServiceBuffer[myDataGroupIndex].SetAnalogDataGroup(m_AnalogTagName, m_AnalogTagValue);
                    _RemoteWebServiceBuffer[myDataGroupIndex].SetAnalogDataGroup(m_AnalogTagName, m_AnalogTagValue);
                    //LocalRealtimeDataService.SetAnalogData(_FactoryOrganizationId, m_LocalAnalogTagName, m_LocalAnalogTagValue, ValidWriteString);
                    //RemoteRealtimeDataService.SetAnalogData(_FactoryOrganizationId, m_RemoteAnalogTagName, m_RemoteAnalogTagValue, ValidWriteString);
                }
            }
            else
            {
                Thread.Sleep(UpdateInterval);
            }
        }
        private DataTable GetRealtimeDataTableName(string myDataBaseName)
        {
            //            string m_Sql = @"SELECT STUFF((SELECT ','+ M.[Name] 
            //                                FROM (SELECT '{0}.dbo.' + N.Name as Name FROM {0}..SysObjects N
            //	                                Where XType='U' and Name like 'Realtime_ProcessVariable%') M
            //	                            FOR XML PATH('')), 1, 1, '') as TableName";
            string m_Sql = @"SELECT '{0}.dbo.' + N.Name as Name FROM {0}..SysObjects N
	                                Where XType='U' and Name like 'Realtime_ProcessVariable%'";
            try
            {
                DataTable m_DataTableNameTable = _dataFactory.Query(string.Format(m_Sql, myDataBaseName));
                return m_DataTableNameTable;
            }
            catch
            {
                return null;
            }
        }
        private DataTable GetRealtimeValueInfo(string myDataTableName)
        {
            string m_Sql = @"Select top 1 * from {0}";
            m_Sql = string.Format(m_Sql, myDataTableName);
            try
            {
                DataTable m_RealtimeValueTable = _dataFactory.Query(m_Sql);
                return m_RealtimeValueTable;
            }
            catch
            {
                return null;
            }
        }
    }

}
