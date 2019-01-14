using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestQS
{
    using System.Configuration;
    public partial class Form1 : Form
    {
        private readonly string pathHistory = AppDomain.CurrentDomain.BaseDirectory + "\\taskHistory.json";

        private Dictionary<string, QSTask> m_historyTasks = new Dictionary<string, QSTask>();
        private Dictionary<string, QSTask> m_currentTasks = new Dictionary<string, QSTask>();
        private List<QSTask> m_needAllerts = new List<QSTask>();

        private Logger m_logger = null;
        private Configuration m_conf = null;
        private QSParser m_parser = null;

        private SMTPMail m_sendSNTPMail = null;
        private string m_instance = string.Empty;
        private bool m_autostart = false;
        public Form1()
        {
            InitializeComponent();
            this.FormClosing += Form1_FormClosing;
            this.m_logger = new Logger(loggerTextBox);
            this.m_conf = new Configuration(this.m_logger);
            this.m_parser = new QSParser(this.m_logger);

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.m_historyTasks = null;
            this.m_currentTasks = null;
            this.m_needAllerts = null;
            this.m_logger.WriteLog("Close application");
            this.m_logger.Close();
            this.m_logger = null;
            this.m_conf = null;
            this.m_parser = null;
            this.m_sendSNTPMail = null;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.m_logger.WriteLog("Start read configuration");
            this.urlTextBox.Text = this.m_conf.ReadSetting("url");
            this.usernameTextBox.Text = this.m_conf.ReadSetting("username");
            bool savePwd = Convert.ToBoolean(this.m_conf.ReadSetting("savePwd"));
            if (this.checkBox1.Checked = savePwd)
                this.pwdTextBox.Text = this.m_conf.ReadSetting("pwd", true);
            this.smtpTextBox.Text = this.m_conf.ReadSetting("smtp_server_address");
            this.portTextBox.Text = this.m_conf.ReadSetting("smtp_server_port");
            bool useSsl = Convert.ToBoolean(this.m_conf.ReadSetting("smtp_server_ssl"));
            this.checkBox2.Checked = useSsl;
            this.smtpuserTextBox.Text = this.m_conf.ReadSetting("smtp_username");
            this.smtppwdTextBox.Text = this.m_conf.ReadSetting("smtp_pwd", true);
            this.emailTextBox.Text = this.m_conf.ReadSetting("smtp_email");
            this.toTextBox.Text = this.m_conf.ReadSetting("smtp_to");

            this.m_instance = this.m_conf.ReadSetting("instance");

            this.m_autostart = Convert.ToBoolean(this.m_conf.ReadSetting("auto_start"));
            if (this.checkBox3.Checked = this.m_autostart)
            {
                this.m_logger.WriteLog("Auto start execute parse");
                this.Shown += Form1_Shown;
            }
            this.button1.Visible = !this.m_autostart;
            this.button4.Visible = !this.m_autostart;

            if (System.IO.File.Exists(pathHistory))
            {
                this.m_logger.WriteLog("Load previous statuses tasks");
                this.m_historyTasks = QSTask.ReadFromFile(pathHistory);
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.Shown -= Form1_Shown;
            btnExecute_Click(null, null);
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.smtpTextBox.Text) &&
                !string.IsNullOrEmpty(this.portTextBox.Text) &&
                !string.IsNullOrEmpty(this.smtpuserTextBox.Text) &&
                !string.IsNullOrEmpty(this.smtppwdTextBox.Text))
                this.m_sendSNTPMail = new SMTPMail(server: this.smtpTextBox.Text,
                    port: Int32.Parse(this.portTextBox.Text),
                    useSsl: this.checkBox2.Checked,
                    username: this.smtpuserTextBox.Text,
                    pwd: this.smtppwdTextBox.Text,
                    logger: this.m_logger);
            else
            {
                this.m_logger.WriteLog("Send mail is not configured.");
                return;
            }

            this.m_logger.WriteLog("Navigate to login form");
            this.webBrowser1.DocumentCompleted += WebBrowser1_startLoad;
            this.webBrowser1.Navigate(urlTextBox.Text);
            Application.DoEvents();
        }

        private void WebBrowser1_startLoad(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.m_logger.WriteLog("Autorization");
            Application.DoEvents();

            this.webBrowser1.DocumentCompleted -= WebBrowser1_startLoad;
            this.webBrowser1.DocumentCompleted += WebBrowser1_NavigateToTasks;

            var element = this.webBrowser1.Document.GetElementById("username-input");
            element.SetAttribute("value", usernameTextBox.Text);
            element = webBrowser1.Document.GetElementById("password-input");
            element.SetAttribute("value", pwdTextBox.Text);
            element = webBrowser1.Document.GetElementById("loginbtn");
            element.InvokeMember("click");
        }

        private void WebBrowser1_NavigateToTasks(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Application.DoEvents();
            webBrowser1.DocumentCompleted -= WebBrowser1_NavigateToTasks;
            webBrowser1.DocumentCompleted += WebBrowser1_GetTaskStatuses;
            webBrowser1.Navigate(urlTextBox.Text + "/tasks");
            m_logger.WriteLog("Navigate to tasks");
        }

        private void WebBrowser1_GetTaskStatuses(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.webBrowser1.DocumentCompleted -= WebBrowser1_GetTaskStatuses;
            new System.Threading.Thread(delegate () { Execute(); }).Start();
        }

        private void Execute()
        {
            System.Threading.Thread.Sleep(7000);
            this.BeginInvoke((MethodInvoker)delegate ()
            {
                this.m_logger.WriteLog("Parse current statuses tasks");
                var table = this.webBrowser1.FindElementByClass("qmc-table-rows");
                if (table == null)
                {
                    this.m_logger.WriteLog("Actual statuses not found");
                    return;
                }
                var rows = table.Children[1].Children;
                this.m_currentTasks = this.m_parser.Parse(rows);

                var toSendAlerst = this.m_parser.Compare(
                    historyTasks: this.m_historyTasks.Values.ToList(),
                    currentTasks: this.m_currentTasks.Values.ToList());

                if (this.m_sendSNTPMail != null)
                    this.m_sendSNTPMail.SendAlerts(toSendAlerst, this.m_instance, this.emailTextBox.Text, this.toTextBox.Text);
                else
                    this.m_logger.WriteLog("Failed tasks not found");

                QSTask.WriteToFile(this.m_currentTasks, pathHistory);

                this.Close();
            });
        }

        private void btnSendTestMessage_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(smtpTextBox.Text) &&
                !string.IsNullOrEmpty(portTextBox.Text) &&
                !string.IsNullOrEmpty(smtpuserTextBox.Text) &&
                !string.IsNullOrEmpty(smtppwdTextBox.Text))
                this.m_sendSNTPMail = new SMTPMail(server: smtpTextBox.Text,
                port: Int32.Parse(portTextBox.Text),
                useSsl: checkBox2.Checked,
                username: smtpuserTextBox.Text,
                pwd: smtppwdTextBox.Text,
                logger: this.m_logger);
            else
            {
                this.m_logger.WriteLog("Send mail is not configured.");
                return;
            }
            if (!string.IsNullOrEmpty(emailTextBox.Text) &&
                !string.IsNullOrEmpty(toTextBox.Text))
                m_sendSNTPMail.SendTestMessage(emailTextBox.Text, toTextBox.Text);
            else
                this.m_logger.WriteLog("Parameters From: and To: are required");
        }

        private void btnSaveConfig_Click(object sender, EventArgs e)
        {
            this.m_logger.WriteLog("Save current settings");
            this.m_conf.WriteSetting("url", urlTextBox.Text);
            this.m_conf.WriteSetting("username", usernameTextBox.Text);
            this.m_conf.WriteSetting("savePwd", Convert.ToString(checkBox1.Checked));
            if (checkBox1.Checked)
                this.m_conf.WriteSetting("pwd", pwdTextBox.Text);
            this.m_conf.WriteSetting("smtp_server_address", smtpTextBox.Text);
            this.m_conf.WriteSetting("smtp_server_port", portTextBox.Text);
            this.m_conf.WriteSetting("smtp_server_ssl", Convert.ToString(checkBox2.Checked));
            this.m_conf.WriteSetting("smtp_username", smtpuserTextBox.Text);
            this.m_conf.WriteSetting("smtp_pwd", smtppwdTextBox.Text);
            this.m_conf.WriteSetting("smtp_email", emailTextBox.Text);
            this.m_conf.WriteSetting("smtp_to", toTextBox.Text);
            this.m_conf.WriteSetting("auto_start", Convert.ToString(checkBox3.Checked));
        }

        private void onCompliteAfterOneMinute_Tick(object sender, EventArgs e)
        {
            if (this.m_autostart)
            {
                this.m_logger.WriteLog("The application failed to collect and process data about failed tasks in one minute.");
                if (this.m_sendSNTPMail == null)
                {
                    this.m_sendSNTPMail.SendAlertOfStop(emailTextBox.Text, toTextBox.Text);
                }
                Application.Exit();
            }
        }
    }
}
