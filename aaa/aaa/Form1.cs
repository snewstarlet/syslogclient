/*
     Name:		syslog client
     Created:	20.09.2021
     Author:	snewstarlet
 */


using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using SyslogNet.Client;
using SyslogNet.Client.Serialization;
using SyslogNet.Client.Transport;

namespace aaa
{
    public partial class Form1 : Form
    {
        
        public string log = "";
        public int i;
        public string ip;
        public int port;

        private SyslogUdpSender _syslogSender;

        public Form1()
        {
            
            InitializeComponent();

        }

        public void Form1_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.Form.CheckForIllegalCrossThreadCalls = false;
            comboBox1.Items.Add("System");
            comboBox1.Items.Add("Application");
            comboBox1.Items.Add("Security");
            comboBox1.Items.Add("Setup");
            comboBox1.Items.Add("Forwarded Events");
            listView1.View = View.Details;
            listView1.Columns.Add("Index", 50, HorizontalAlignment.Left);
            listView1.Columns.Add("InstanceId", 80, HorizontalAlignment.Left);
            listView1.Columns.Add("Message", 300 , HorizontalAlignment.Left);
            listView1.Columns.Add("Source", 280, HorizontalAlignment.Left);
            listView1.Columns.Add("Time Generated", 120, HorizontalAlignment.Left);

        }

        public void button1_Click(object sender, EventArgs e)
        {
            try
            {
                log = comboBox1.Text;
                Thread trd = new Thread(new ThreadStart(this.vericek));
                trd.IsBackground = true;
                trd.Start();
                button1.Enabled = false;
            }
            catch(Exception err)
            {
                MessageBox.Show("Hata: " + err.Message);
                button1.Enabled = true;
            }
        }

        public void vericek()
        {
            ip = textBox1.Text;
            port = Int32.Parse(textBox2.Text);
            while (true)
            {
                try
                {
                    EventLog demoLog = new EventLog(log);
                    EventLogEntryCollection entries = demoLog.Entries;
                    DateTime dt = DateTime.Now.AddSeconds(-1);
                    foreach (EventLogEntry entry in entries)
                    {
                        if (entry.TimeGenerated > dt && i != entry.Index)
                        {   
                            int sira = listView1.Items.Count;
                            listView1.Items.Add(entry.Index.ToString());
                            listView1.Items[sira].SubItems.Add(entry.InstanceId.ToString());
                            listView1.Items[sira].SubItems.Add(entry.Message.ToString());
                            listView1.Items[sira].SubItems.Add(entry.Source.ToString());
                            listView1.Items[sira].SubItems.Add(entry.TimeGenerated.ToString());
                            i = entry.Index;
                            string msg = entry.EntryType.ToString() +" | "+ entry.Message.ToString() + " | " + entry.Source.ToString();
                            _syslogSender = new SyslogUdpSender(ip, port);
                            _syslogSender.Send(
                                new SyslogMessage(
                                    entry.TimeGenerated,
                                    Facility.SecurityOrAuthorizationMessages1,
                                    Severity.Informational,
                                    Environment.MachineName,
                                    "snewstarlet's syslog client ",
                                    msg),
                                new SyslogRfc3164MessageSerializer());
                        }
                    }
                }
                catch (Exception err)
                {

                    MessageBox.Show("Hata: " + err.Message);
                    button1.Enabled = true;
                    break;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
        }
    }
}
