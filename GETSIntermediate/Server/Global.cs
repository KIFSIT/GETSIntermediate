using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using GETSIntermediate.Server;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Runtime.InteropServices;



namespace GETSIntermediate
{
    // Market Data Delegates
    public delegate void TCPTerminal_MessageRecivedDel(Socket socket, byte[] message);
    public delegate void TCPTerminal_ConnectDel(Socket socket);
    public delegate void TCPTerminal_DisconnectDel(Socket socket);

    // Trade Info Delegates

    public delegate void TradeTerminal_MessageRecivedDel(Socket socket, byte[] message);
    public delegate void TradeTerminal_ConnectDel(Socket socket);
    public delegate void TradeTerminal_DisconnectDel(Socket socket);

    // GUI Info Delegates
    public delegate void GUITerminal_MessageRecivedDel(Socket socket, byte[] message);
    public delegate void GUITerminal_ConnectDel(Socket socket);
    public delegate void GUITerminal_DisconnectDel(Socket socket);

    // RMS Info Delegates
    public delegate void RMSTerminal_MessageRecivedDel(Socket socket, byte[] message);
    public delegate void RMSTerminal_ConnectDel(Socket socket);
    public delegate void RMSTerminal_DisconnectDel(Socket socket);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="message"></param>
    public delegate void MKTTerminal_MessageRecivedDel(Socket socket, byte[] message);
    public delegate void MKTTerminal_ConnectDel(Socket socket);
    public delegate void MKTTerminal_DisconnectDel(Socket socket);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="message"></param>
    public delegate void RMS_MessageRecivedDel(Socket socket, byte[] message);
    public delegate void RMS_ConnectDel(Socket socket);
    public delegate void RMS_DisconnectDel(Socket socket);

    public class Global
    {
        public static Form1 frmWatch;
        public static RMSSocketManager connection1;
        public static ClientSocketManager Mkt;
        public static SystemConfiguration SystemConfig;
        public static List<string> Name_Client;
        public static object _dataLock;
        public static object _MktLock;
        // lock for crashing intermediate 
        
        public static object _dataReceiverfrmServer;
        public static object _dataSendToClient;
        public static object _dataSendfrmClient;
        //public static  object _rms;
        public static readonly object _rms = new object();
        public static int LengthBuffer;
        public static DataSet DsContract;
        public static UInt64 UniqueID;
        public static bool Gui_Flg;
        public static List<int> Gui_no;
        public static string password;

        public static bool isReadytoForward = false;

        public static SqlConnectionLocal sqlConnectionObj;
        public static SqlCeConnection sqlConn;

        public static List<string> updatedColumn = new List<string>();


    }
}