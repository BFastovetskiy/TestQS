using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestQS
{
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Runtime.Serialization;

    /// <summary>
    /// Class that describes the status task in Qlik Sense
    /// </summary>
    [DataContract]
    public class QSTask : ICloneable
    {
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="id">Id Qlik Sense task</param>
        /// <param name="alreadySent">Failure notification has already been sent.</param>
        public QSTask(string id, bool alreadySent = false)
        {
            this.AlreadySent = alreadySent;
            this.Id = id;
        }

        /// <summary>
        /// Id Qlik Sense task
        /// </summary>
        [DataMember]
        public string Id { get; private set; }

        /// <summary>
        /// Name Qlik Sense task
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Associated resource Qlik Sense task
        /// </summary>
        [DataMember]
        public string AssociatedResource { get; set; }

        /// <summary>
        /// Type Qlik Sense task
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// Enable Qlik Sense task
        /// </summary>
        [DataMember]
        public string Enable { get; set; }

        /// <summary>
        /// Status last execution Qlik Sense task
        /// </summary>
        [DataMember]
        public string Status { get; set; }

        /// <summary>
        /// Last execution Qlik Sense task
        /// </summary>
        [DataMember]
        public string LastExecution { get; set; }

        /// <summary>
        /// Next execution Qlik Sense task
        /// </summary>
        [DataMember]
        public string NextExecution { get; set; }

        /// <summary>
        /// Failure notification has already been sent.
        /// Default: false
        /// Not used in current version application.
        /// </summary>
        [DataMember]
        public bool AlreadySent { get; set; }

        /// <summary>
        /// Method of serializing the collection QSTask to disk
        /// </summary>
        /// <param name="tasks">Collection QSTask</param>
        /// <param name="filename">File name to save</param>
        public static void WriteToFile(Dictionary<string, QSTask> tasks, string filename)
        {
            StringBuilder sb = new StringBuilder();
            foreach (QSTask task in tasks.Values)
            {
                sb.AppendLine(QSTask.ConvertoToJSON(task));
            }
            File.WriteAllText(filename, sb.ToString());
        }

        /// <summary>
        /// Method of deserializing the collection QSTask from disk
        /// </summary>
        /// <param name="filename">File name to read data</param>
        /// <returns>Collection QSTask</returns>
        public static Dictionary<string, QSTask> ReadFromFile(string filename)
        {
            Dictionary<string, QSTask> tasks = new Dictionary<string, QSTask>();

            var jsons = File.ReadAllLines(filename);
            foreach (var json in jsons)
            {
                var task = QSTask.ConvertoFromJSON(json);
                tasks.Add(task.Id, task);
            }

            return tasks;
        }

        /// <summary>
        /// Clone QTask object
        /// </summary>
        /// <param name="task">source QTask</param>
        /// <returns>New QTask</returns>
        public static QSTask Clone(QSTask task)
        {
            return new QSTask(task.Id, task.AlreadySent)
            {
                AssociatedResource = task.AssociatedResource,
                Enable = task.Enable,
                LastExecution = task.LastExecution,
                Name = task.Name,
                NextExecution = task.NextExecution,
                Status = task.Status,
                Type = task.Type
            };
        }

        /// <summary>
        /// Method for serializing object to JSON
        /// </summary>
        /// <param name="task">QSTask</param>
        /// <returns>JSON string</returns>
        private static string ConvertoToJSON(QSTask task)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(QSTask));
                jsonFormatter.WriteObject(stream, task);
                stream.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Method for deserializing object from JSON
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>QSTask</returns>
        private static QSTask ConvertoFromJSON(string json)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(json);
                writer.Flush();

                stream.Seek(0, SeekOrigin.Begin);
                DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(QSTask));
                return (QSTask)jsonFormatter.ReadObject(stream);
            }
                
            
        }

        /// <summary>
        /// Clone current object QTask
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return QSTask.Clone(this);
        }
    }
}
