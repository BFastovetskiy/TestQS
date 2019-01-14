using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestQS
{
    /// <summary>
    /// Class parser
    /// Analyzes html elements
    /// Compares collections of objects with each other.
    /// </summary>
    internal class QSParser
    {
        private Logger m_logger = null;

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="logger">application log writer</param>
        public QSParser(Logger logger)
        {
            this.m_logger = logger;
        }

        /// <summary>
        /// Method analyze collection html elements
        /// </summary>
        /// <param name="elements">Сollection html elements</param>
        /// <returns>Сollection QSTask</returns>
        public Dictionary<string, QSTask> Parse(HtmlElementCollection elements)
        {
            this.m_logger.WriteLog("Start parsing html elements");
            Dictionary<string, QSTask> result = new Dictionary<string, QSTask>();
            foreach (HtmlElement row in elements)
            {
                var id = row.GetAttribute("id");
                QSTask task = new QSTask(id)
                {
                    Name = row.Children[0].GetAttribute("title"),
                    AssociatedResource = row.Children[1].InnerText,
                    Type = row.Children[2].InnerText,
                    Enable = row.Children[3].InnerText,
                    Status = row.Children[4].Children[1].Children[0].InnerText,
                    LastExecution = row.Children[5].GetAttribute("title"),
                    NextExecution = row.Children[6].InnerText
                };

                result.Add(task.Id, task);
            }
            this.m_logger.WriteLog("End parsing html elements");
            this.m_logger.WriteLog(string.Format("Found {0} QTasks", result.Count));
            return result;
        }

        /// <summary>
        /// Compares collections of objects with each other.
        /// </summary>
        /// <param name="historyTasks">Historical data on the status of Qlik Sense tasks</param>
        /// <param name="currentTasks">Current Data on the status of Qlik Sense tasks</param>
        /// <returns>Resulting QSTask collection</returns>
        public List<QSTask> Compare(List<QSTask> historyTasks, List<QSTask> currentTasks)
        {
            List<QSTask> result = null;
            this.m_logger.WriteLog("Compare history and current iterations");
            if (historyTasks.Count != 0)
                result = currentTasks.Except<QSTask>(currentTasks).ToList().Where(t => t.Status == "Failed" && t.Enable == "Yes").ToList();            
            else
                result = currentTasks.Where(t => t.Status == "Failed" && t.Enable == "Yes").ToList();
            this.m_logger.WriteLog(string.Format("Found {0} of failed tasks", result.Count));
            return result;
        }
    }
}
