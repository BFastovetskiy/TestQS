using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace TestQS
{
    /// <summary>
    /// Class for sending emails
    /// </summary>
    internal class SMTPMail
    {
        public Logger m_logger = null;

        /// <summary>
        /// Smtp server address.
        /// DNS name or IP address (not checked).
        /// </summary>
        public string Server { get; set; }
        /// <summary>
        /// Smtp server port.
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// Use SSL for send message 
        /// </summary>
        public bool UseSSL { get; set; }

        /// <summary>
        /// SMTP account
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// SMTP account password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="logger">application log writer</param>
        public SMTPMail(Logger logger)
        {
            this.m_logger = logger;
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="logger">Application log writer</param>
        /// <param name="server">Smtp server address</param>
        /// <param name="port">Smtp server port</param>
        /// <param name="useSsl">Use ssl from send message over smtp</param>
        /// <param name="username">Account for smtp</param>
        /// <param name="pwd">Password for smtp</param>
        public SMTPMail(Logger logger, string server, int port, bool useSsl, string username, string pwd)
        {
            this.m_logger = logger;
            this.Server = server;
            this.Port = port;
            this.UseSSL = useSsl;
            this.Username = username;
            this.Password = pwd;
        }

        /// <summary>
        /// Method for sest message sending method
        /// </summary>
        /// <param name="from">From</param>
        /// <param name="to">To</param>
        public void SendTestMessage(string from, string to)
        {
            using (MailMessage mail = new MailMessage(from, to))
            {
                mail.BodyEncoding = UTF8Encoding.UTF8;
                mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Host = this.Server;
                smtpClient.Port = this.Port;
                smtpClient.Timeout = 10000;
                smtpClient.EnableSsl = this.UseSSL;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential(this.Username, this.Password);

                mail.IsBodyHtml = true;
                mail.Subject = "This is a test message from TestQS";
                mail.Body = "This is a test message";
                try
                {
                    smtpClient.Send(mail);
                    this.m_logger.WriteLog("Sent test message success");
                }
                catch (Exception ex)
                {
                    this.m_logger.WriteLog(ex.Message);
                }
            }
        }

        /// <summary>
        /// Method for sending results of Qlik Sense tasks status analysis
        /// </summary>
        /// <param name="tasks">Collection QSTask</param>
        /// <param name="instance">Instance Qlik Sense</param>
        /// <param name="from">From</param>
        /// <param name="to">To</param>
        public void SendAlerts(List<QSTask> tasks, string instance, string from, string to)
        {
            string mailTitle = string.Format("Qlik Sense alerts of failed tasks ({0})", instance);
            if (tasks.Count == 0)
            {
                this.m_logger.WriteLog("The failed tasks for send not found");
                return;
            }

            string body = "";
            body += "<h2>Qlik Sense failed tasks</h2>";

            foreach (var task in tasks)
            {
                body += string.Format("<h3>Task: {0}</h3>", task.Name);
                body += string.Format("<p>Id: {0} <br/>", task.Id);
                body += string.Format("Associated resource: {0} <br/>", task.AssociatedResource);
                body += string.Format("Type: {0} <br/>", task.Type);                
                body += string.Format("Enable: {0} <br/>", task.Enable);
                body += string.Format("Last execution: {0} <br/>", task.LastExecution);
                body += string.Format("Status: {0} <br/>", task.Status);
                body += string.Format("Next execution: {0} </p>", task.NextExecution);
            }

            using (MailMessage mail = new MailMessage(from, to))
            {
                mail.BodyEncoding = UTF8Encoding.UTF8;
                mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Host = this.Server;
                smtpClient.Port = this.Port;
                smtpClient.Timeout = 10000;
                smtpClient.EnableSsl = this.UseSSL;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential(this.Username, this.Password);

                mail.Subject = mailTitle;
                mail.IsBodyHtml = true;
                mail.Body = body;
                try
                {
                    smtpClient.Send(mail);
                    this.m_logger.WriteLog("Sent alert message on failed to process statuses tasks");
                }
                catch (Exception ex)
                {
                    this.m_logger.WriteLog(ex.Message);
                }
            }

            this.m_logger.WriteLog("Send email alerts of failed tasks");
        }

        /// <summary>
        /// Method notifies of a failed attempt to collect data from Qlik Sense
        /// </summary>
        /// <param name="from">From</param>
        /// <param name="to">To</param>
        public void SendAlertOfStop(string from, string to)
        {
            using (MailMessage mail = new MailMessage(from, to))
            {
                mail.BodyEncoding = UTF8Encoding.UTF8;
                mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Host = this.Server;
                smtpClient.Port = this.Port;
                smtpClient.Timeout = 10000;
                smtpClient.EnableSsl = this.UseSSL;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential(this.Username, this.Password);

                mail.Subject = "TestQS failed to process statuses tasks";
                mail.IsBodyHtml = true;
                mail.Body = "<p>The application failed to collect and process data about failed tasks in one minute.</p>";
                try
                {
                    smtpClient.Send(mail);
                    this.m_logger.WriteLog("Sent alert message on failed to process statuses tasks");
                }
                catch (Exception ex)
                {
                    this.m_logger.WriteLog(ex.Message);
                }
            }
        }
    }
}
