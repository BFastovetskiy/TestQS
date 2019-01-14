using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestQS
{
    /// <summary>
    /// Class for logging events in the application
    /// </summary>
    internal class Logger : IDisposable
    {
        
        private TextBox m_log = null;
        private string pathLogFile = AppDomain.CurrentDomain.BaseDirectory + string.Format(@"{0}_testqs.log", DateTime.Now.ToString("yyyy-MM-dd"));
        private System.IO.StreamWriter m_fileLog = null;
        private int m_quantityDaysHistory = 0;

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="textBox">Control to display events on the form</param>
        /// <param name="quantityDaysHistory">Quantity of days to keep event history</param>
        public Logger(TextBox textBox, int quantityDaysHistory = 7)
        {
            this.m_quantityDaysHistory = quantityDaysHistory;
            this.m_log = textBox;
            this.m_log.AddLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + "Log file: " + pathLogFile);
            this.m_fileLog = System.IO.File.AppendText(pathLogFile);
            CLearHistory();
        }

        public void Dispose()
        {
            this.m_fileLog.Dispose();
            this.m_fileLog = null;
        }

        /// <summary>
        /// Write a message to the event log
        /// </summary>
        /// <param name="logMessage">Message</param>
        public void WriteLog(string logMessage)
        {
            this.m_log.AddLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + logMessage);
            this.m_fileLog.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + logMessage);
        }

        /// <summary>
        /// Close event log. Forced write to disk.
        /// </summary>
        public void Close()
        {
            this.m_fileLog.Flush();
            this.m_fileLog.Close();
        }

        /// <summary>
        /// Method for clearing the event history. Older than the number of days specified when creating the object.
        /// </summary>
        public void CLearHistory()
        {
            CLearHistory(this.m_quantityDaysHistory);
        }

        /// <summary>
        /// Method for clearing the event history.
        /// </summary>
        /// <param name="quantityDaysHistory">The number of days for which there is a history of events</param>
        public void CLearHistory(int quantityDaysHistory)
        {
            var logFiles = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.log");

            Dictionary<DateTime, string> files = new Dictionary<DateTime, string>();
            foreach (var file in logFiles)
            {
                files.Add(Convert.ToDateTime(System.IO.Path.GetFileName(file).Substring(0, 10)), file);
            }
            var dates = files.Keys.Where(f => f < DateTime.Now.AddDays(-quantityDaysHistory)).ToList<DateTime>();

            foreach (var date in dates)
            {
                try
                {
                    System.IO.File.Delete(files[date]);
                    this.m_log.AddLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + string.Format("Log history older than {0} days cleaned", this.m_quantityDaysHistory));
                }
                catch (Exception ex)
                {
                    this.m_log.AddLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + "WARNING! Error clearing history. File: " + files[date]);
                    this.m_log.AddLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + ex.Message);
                }

            }
        }
    }
}
