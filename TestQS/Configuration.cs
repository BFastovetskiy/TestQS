using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestQS
{
    using System.Configuration;
    internal class Configuration
    {
        private Logger m_logger = null;

        public Configuration(Logger logger)
        {
            this.m_logger = logger;
        }

        private void ReadAllSettings()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;

                if (appSettings.Count == 0)
                {
                    if (this.m_logger != null)
                        this.m_logger.WriteLog("AppSettings is empty.");
                }
                else
                {
                    foreach (var key in appSettings.AllKeys)
                    {
                        if (this.m_logger != null)
                            this.m_logger.WriteLog(string.Format("Key: {0} Value: {1}", key, appSettings[key]));
                    }
                }
            }
            catch (ConfigurationErrorsException)
            {
                if (this.m_logger != null)
                    this.m_logger.WriteLog("Error reading app settings");
            }
        }

        public string ReadSetting(string key, bool isPassword = false)
        {
            string result = string.Empty;
            try
            {
                result = ConfigurationManager.AppSettings[key];
                if (this.m_logger != null)
                {
                    if (isPassword)
                        this.m_logger.WriteLog(string.Format("Key: {0} Value: {1}", key, "*****************"));
                    else
                        this.m_logger.WriteLog(string.Format("Key: {0} Value: {1}", key, result));
                }
                    
            }
            catch (ConfigurationErrorsException)
            {
                if (this.m_logger != null)
                    this.m_logger.WriteLog(string.Format("Error reading key {0} in app settings", key));
            }
            return result;
        }

        public void WriteSetting(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                if (this.m_logger != null)
                    this.m_logger.WriteLog(string.Format("Write value for key {0} to setting file", key));
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                if (this.m_logger != null)
                    this.m_logger.WriteLog(string.Format("Error save key {0} in app settings", key));
            }
        }

    }
}
