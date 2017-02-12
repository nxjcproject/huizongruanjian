using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balance.Infrastructure.Configuration
{
    public class ConfigService
    {
        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetConfig(string key)
        {
            string _value = string.Empty;
            if (ExeAppFileAddress == "")
            {
                System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings[key] != null)
                {
                    _value = config.AppSettings.Settings[key].Value;
                }
            }
            else
            {
                System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ExeAppFileAddress);
                if (config.HasFile)
                {
                    if (config.AppSettings.Settings[key] != null)
                    {
                        _value = config.AppSettings.Settings[key].Value;
                    }
                }
            }
            return _value;
        }
        public static string ExeAppFileAddress
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["ExeAppFileAddress"] != null ? ConfigurationManager.AppSettings["ExeAppFileAddress"].ToString() : "";
                }
                catch
                {
                    return "";
                }
            }
        }
        public static string[] FactoryOrganizationId
        {
            get
            {
                string m_FactoryOrganization = ConfigurationManager.AppSettings["FactoryID"] != null ? ConfigurationManager.AppSettings["FactoryID"].ToString() : "";
                return m_FactoryOrganization.Split(',');
            }
        }
        public static int UpdateInterval
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["UpdateInterval"] != null ? Int32.Parse(ConfigurationManager.AppSettings["UpdateInterval"].ToString()) : 1000;
                }
                catch
                {
                    return 1000;
                }
            }
        }
        public static int MinQueryInterval
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["MinQueryInterval"] != null ? Int32.Parse(ConfigurationManager.AppSettings["MinQueryInterval"].ToString()) : 5;
                }
                catch
                {
                    return 5;
                }
            }
        }
        public static int DBThreadItemsCount
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["DBThreadItemsCount"] != null ? Int32.Parse(ConfigurationManager.AppSettings["DBThreadItemsCount"].ToString()) : 5;
                }
                catch
                {
                    return 5;
                }
            }
        }
        public static string GroupIpAddress
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["GroupIpAddress"] != null ? ConfigurationManager.AppSettings["GroupIpAddress"].ToString() : "127.0.0.1";
                }
                catch
                {
                    return "127.0.0.1";
                }
            }
        }
    }
}
