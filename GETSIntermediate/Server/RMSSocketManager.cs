using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using GETSIntermediate.Server;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.Net.Sockets;
using System.Net;

namespace GETSIntermediate.Server
{
    public class RMSSocketManager
    {
        ClientSocketManager mkt = new ClientSocketManager();

        public event MKTTerminal_MessageRecivedDel MKTMessageRecived;
        public event MKTTerminal_ConnectDel MKTClientConnect;
        public event MKTTerminal_DisconnectDel MKTClientDisconnect;

        public event RMS_MessageRecivedDel RMSReceivedMsg;
        public event RMS_ConnectDel RMSConnect;
        public event RMS_DisconnectDel RMSDisconnected;

        private Socket R_Socket;
        public byte[] dataBuffer;
      
        private void RMSRaiseClientConnected(Socket socket)
        {
            if (RMSConnect != null)
            {
                RMSConnect(socket);
            }
        }

        public void StartServer()
        {
            //_tcpGUIPort = new Tcp(Global.SystemConfig.RMSSendIP, Global.SystemConfig.RMSSendPort, "", TypeOfCompression.None);            
            //_tcpGUIPort.DataArrival += new Tcp.DataArrivalHandler(_tcpGUIPort_DataArrival);
            //_tcpGUIPort.Connect += new Tcp.ConnectHandler(_tcpGUIPort_Connect);
            //_tcpGUIPort.Disconnect += new Tcp.DisconnectHandler(_tcpGUIPort_Disconnect);
            //_tcpGUIPort.Accept += new Tcp.AcceptHandler(_tcpGUIPort_Accept); 
            //_tcpGUIPort.ListeningStart();  
            
        }

        //void _tcpGUIPort_Accept(ConnectionData connectionData)
        //{
        //    TransactionWatch.TransactionMessage(string.Format("Server MarketData has been connected!"), Color.Red);
        //    TransactionWatch.TransactionMessage(string.Format("Server GUIPort has been connected!"), Color.Red);
        //    _setRmsconnection();
        //}

        void _setRmsconnection()
        {
            IPAddress ip = IPAddress.Parse(Global.SystemConfig.RMSListenIP);
            IPEndPoint ipLocal = new IPEndPoint(ip, Global.SystemConfig.RMSListenPort);
            R_Socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
            R_Socket.BeginConnect(ipLocal, new AsyncCallback(OnRMSConnection), R_Socket);
        }

        private void OnRMSConnection(IAsyncResult asyn)
        {
            Socket ClientSocket = (Socket)asyn.AsyncState;
            if (ClientSocket.Connected)
            {
                ClientSocket.EndConnect(asyn);
            }
            RMSRaiseClientConnected(ClientSocket);
            RMSListenSocketHandler RMSConnect = new RMSListenSocketHandler(ClientSocket);
            RMSConnect.RMSMessageRecived += OnRMSMessageRecived;
            RMSConnect.RMSClientDisconnect += RMSConnect_Disconnected;
            RMSConnect.StartListen();
            TransactionWatch.TransactionMessage(string.Format("Server RMS has been connected!"), Color.Red);
            createRMSTerminal(Global.SystemConfig.ClientListenPort);
        }

        private void createRMSTerminal(int alPort)
        {
            mkt = new ClientSocketManager();
            mkt.RMSMessageRecived += new RMSTerminal_MessageRecivedDel(m_ServerTerminal_RMSMessageRecived);
            mkt.RMSClientConnect += new RMSTerminal_ConnectDel(m_ServerTerminal_RMSClientConnect);
            mkt.RMSClientDisconnect += new RMSTerminal_DisconnectDel(m_ServerTerminal_RMSClientDisconnect);
            mkt.RMSStartListen(alPort);
        }

        void m_ServerTerminal_RMSMessageRecived(System.Net.Sockets.Socket socket, byte[] message)
        {

        }

        void m_ServerTerminal_RMSClientDisconnect(System.Net.Sockets.Socket socket)
        {

        }

        void m_ServerTerminal_RMSClientConnect(System.Net.Sockets.Socket socket)
        {

        }


        void OnRMSMessageRecived(Socket socket, byte[] message)
        {
            lock (Global._rms)
            {
                if (MKTMessageRecived != null)
                {
                    Global.Mkt.RMSDistributeMessage(message);
                }
            }
        }

        void RMSConnect_Disconnected(Socket socket)
        {
            TransactionWatch.TransactionMessage(string.Format("Connection Close !!!!!"), Color.Red);
        }
   

        public T PinnedPacket<T>(byte[] data)
        {
            object packet = new object();
            try
            {
                //  int length = Marshal.SizeOf(data);
                GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                IntPtr IntPtrOfObject = handle.AddrOfPinnedObject();
                packet = Marshal.PtrToStructure(IntPtrOfObject, typeof(T));
                handle.Free();
            }
            catch (Exception)
            {

            }
            return (T)packet;
        }

        #region Member variables

        public TcpClient tcpClient;
           //Tcp _tcpMarketData;
        //   public Tcp _tcpRMS;
         // public Tcp _tcpGUIPort;
         //  public Tcp _tcpRMSPort;
           private object _dataLock;
        #endregion
    }
}
