using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GETSIntermediate.Server;
using System.Xml.Serialization;
using System.Xml;

namespace GETSIntermediate
{
    public partial class Form1 : Form
    {
        //second commit
        ClientSocketManager m_ServerTerminal;
        internal StreamWriter _errorLog;
        internal StreamWriter _transactionLog;
        internal object _errorFileLock;
        internal object _transactionFileLock;
        public delegate void SystemUpdates(string message);

        public Form1()
        {
            InitializeComponent();
            //Global.DsContract = new DataSet();
            InitializeAPI();
            //Global.Mkt = new ClientSocketManager();
            Global.connection1 = new Server.RMSSocketManager();
            Global.connection1.MKTClientConnect += new MKTTerminal_ConnectDel(connection1_MKTClientConnect);
            Global.connection1.MKTClientDisconnect += new MKTTerminal_DisconnectDel(connection1_MKTClientDisconnect);
            Global.connection1.MKTMessageRecived += new MKTTerminal_MessageRecivedDel(connection1_MKTMessageRecived);
            //ReadVersionFile();
         
        }

        void connection1_MKTMessageRecived(System.Net.Sockets.Socket socket, byte[] message)
        {
            throw new NotImplementedException();
        }

        void connection1_MKTClientDisconnect(System.Net.Sockets.Socket socket)
        {
            throw new NotImplementedException();
        }

        void connection1_MKTClientConnect(System.Net.Sockets.Socket socket)
        {
            throw new NotImplementedException();
        }

        void ReadVersionFile()
        {
            string Version = "";
            string VersionFile = AppDomain.CurrentDomain.BaseDirectory + "\\" + "Version.txt";
            if (File.Exists(VersionFile))
            {
                using (StreamReader sr = File.OpenText(VersionFile))
                {
                    string s = String.Empty;
                    while ((s = sr.ReadLine()) != null)
                    {
                        //do minimal amount of work here
                        Version = s.ToString();
                    }
                }
            }
            this.Text = "Con_Rev Version " + Version;
            // lblVersion.Text = Version.ToString();
        }

        void WriteToTransactionWatch(string msg, Color color)
        {
            try
            {
                TransactionWatch.TransactionMessage(msg, color);
            }
            catch (Exception) { }
        }

        public bool InitializeAPI()
        {
            try
            {
                Global._dataLock = new object();
                Global._MktLock = new object();
                Global._dataReceiverfrmServer = new object();
                Global._dataSendfrmClient = new object();
                Global._dataSendToClient = new object();

                _errorFileLock = new object();
                _transactionFileLock = new object();
                Global.SystemConfig = new SystemConfiguration();
                ReadSystemConfiguration();
                GenerateLogFiles();
            }
            catch (FileNotFoundException)
            {
                return CreateNewSystemConfiguration();
            }
            return true;
        }

        internal bool CreateNewSystemConfiguration()
        {
            try
            {
                Global.SystemConfig.ApplicationName = "GETSInermediate";
                Global.SystemConfig.RMSSendIP = "172.16.2.91";
                Global.SystemConfig.RMSSendPort = 4001;
                Global.SystemConfig.RMSListenIP = "172.16.2.91";
                Global.SystemConfig.RMSListenPort = 4002;
                Global.SystemConfig.ClientListenIP = "172.16.2.91";
                Global.SystemConfig.ClientListenPort = 2010;
                Global.SystemConfig.RmsConnect = false;
                SaveSystemConfiguration();
                MessageBox.Show("Now Configuration is Created please Restart Application!!!", "Configuration", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                    MessageBoxDefaultButton.Button1);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        internal static string FilePath
        {
            get
            {
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\" + "Config"))
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\" + "Config");
                return AppDomain.CurrentDomain.BaseDirectory + "\\" + "Config" + "\\" + "SystemConfig.xml";
            }
        }

        internal void ReadSystemConfiguration()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(SystemConfiguration));
            FileStream fileStream = new FileStream(FilePath, FileMode.Open);
            Global.SystemConfig = xmlSerializer.Deserialize(fileStream) as SystemConfiguration;
            fileStream.Close();
        }

        internal bool SaveSystemConfiguration()
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(SystemConfiguration));
                StreamWriter streamWriter = new StreamWriter(FilePath);
                xmlSerializer.Serialize(streamWriter, Global.SystemConfig);
                streamWriter.Close();
            }
            catch (XmlException)
            {
                MessageBox.Show("Unable to save data to System configuration file.", "API", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                    MessageBoxDefaultButton.Button1);
                return false;
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to access the System configuration file.", "API", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                    MessageBoxDefaultButton.Button1);
                return false;
            }
            return true;
        }

        internal void GenerateLogFiles()
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\" + "Logs" + "\\";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string date = DateTime.Now.ToString("ddMMMyyyy_hh_mm_ss") + ".txt";

                string fileName = path + "ErrorLog" + "-" + date;
                _errorLog = new StreamWriter(fileName, true);
                _errorLog.AutoFlush = true;

                fileName = path + "TransactionLog" + "-" + date;
                _transactionLog = new StreamWriter(fileName, true);
                _transactionLog.AutoFlush = true;
            }
            catch (Exception)
            { }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Global.Mkt = new ClientSocketManager();
            //if (Global.SystemConfig.RmsConnect == true)
            //{
            //    Global.rmsIntermediateConn._setRMSConnection(Global.SystemConfig.RMSSendIP, Global.SystemConfig.RMSSendPort);
            //}
            if (Global.SystemConfig.RmsConnect == true)
            {
                createRMSTerminal(Global.SystemConfig.RMSListenPort);
            }
            Global.UniqueID = 1;
            Global.sqlConnectionObj = new SqlConnectionLocal();
            Global.sqlConn = Global.sqlConnectionObj.Connect();
        }


        public void WriteToErrorLog(string message)
        {
            try
            {
                lock (_errorFileLock)
                {
                    _errorLog.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:ffff >> ") + message);
                }
            }
            catch (Exception)
            {
            }
        }

        private void RMSToolStripMenuItem_Click(object sender, EventArgs e)
        {
          //  createRMSTerminal(Global.SystemConfig.RMSListenPort);
        }

        private void GUIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Global.Mkt.GUIClientConnect += new GUITerminal_ConnectDel(m_ServerTerminal_GUIClientConnect);
            //Global.Mkt.GUIMessageRecived += new GUITerminal_MessageRecivedDel(m_ServerTerminal_GUIMessageRecived);
        }

        void m_ServerTerminal_GUIMessageRecived(System.Net.Sockets.Socket socket, byte[] message)
        {
            throw new NotImplementedException();
        }

        void m_ServerTerminal_GUIClientConnect(System.Net.Sockets.Socket socket)
        {
            throw new NotImplementedException();
        }

        private void createRMSTerminal(int alPort)
        {
            m_ServerTerminal = new ClientSocketManager();
            m_ServerTerminal.RMSMessageRecived += new RMSTerminal_MessageRecivedDel(m_ServerTerminal_RMSMessageRecived);
            m_ServerTerminal.RMSClientConnect += new RMSTerminal_ConnectDel(m_ServerTerminal_RMSClientConnect);
            m_ServerTerminal.RMSClientDisconnect += new RMSTerminal_DisconnectDel(m_ServerTerminal_RMSClientDisconnect);
            m_ServerTerminal.RMSStartListen(alPort);
        }
        void m_ServerTerminal_RMSClientDisconnect(System.Net.Sockets.Socket socket)
        {
        }

        void m_ServerTerminal_RMSClientConnect(System.Net.Sockets.Socket socket)
        {
        }

        void m_ServerTerminal_RMSMessageRecived(System.Net.Sockets.Socket socket, byte[] message)
        {
        }

    }
}
