using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GETSIntermediate.Server
{
    public class ClientSocketManager
    {
        //ClientSocketManager m_ServerTerminal;
        public event TCPTerminal_MessageRecivedDel MessageRecived;
        public event TCPTerminal_ConnectDel ClientConnect;
        public event TCPTerminal_DisconnectDel ClientDisconnect;

        public event GUITerminal_MessageRecivedDel GUIMessageRecived;
        public event GUITerminal_ConnectDel GUIClientConnect;
        public event GUITerminal_DisconnectDel GUIClientDisconnect;


        public event RMSTerminal_MessageRecivedDel RMSMessageRecived;
        public event RMSTerminal_ConnectDel RMSClientConnect;
        public event RMSTerminal_DisconnectDel RMSClientDisconnect;      
        // Thread Signal
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public  Socket m_socket;
        public Socket market_socket;
        public Socket t_socket;
        public  Socket r_socket;
        private bool m_Closed;
        

        public Dictionary<long, RMSSendSocketHandler> m_Servers =
            new Dictionary<long, RMSSendSocketHandler>();

        public  Dictionary<long, ClientListenSocketHandler> R_clients =
           new Dictionary<long, ClientListenSocketHandler>();

        
        // Removing GUI ID we req this
        public Dictionary<long, long> R_gui_info = new Dictionary<long, long>();
                  
        public void RMSStartListen(int port)
        {
            IPAddress ip = IPAddress.Parse(Global.SystemConfig.RMSListenIP);
            IPEndPoint ipLocal = new IPEndPoint(ip, port);
            r_socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
          
            try
            {
                r_socket.Bind(ipLocal);
            }
            catch (Exception ex)
            {
                ////Debug.Fail(ex.ToString(),
                //    string.Format("Can't connect to port {0}!", port));

                return;
            }
            //start listening...
            r_socket.Listen(4);
            // create the call back for any client connections...
            r_socket.BeginAccept(new AsyncCallback(OnClientListenSocketAccept), null);
            TransactionWatch.TransactionMessage("RMS Server is available for client on port "+ port, Color.Red); 
        }

        private void OnClientListenSocketAccept(IAsyncResult asyn)
        {
            if (m_Closed)
            {
                return;
            }
            try
            {
                lock (Global._rms)
                {
                    Socket clientSocket = r_socket.EndAccept(asyn);
                    RaiseRMSClientConnected(clientSocket);
                    
                    ClientListenSocketHandler connectedClient = new ClientListenSocketHandler(clientSocket);
                    connectedClient.MessageRecived += OnRMSMessageRecived;
                    connectedClient.Disconnected += OnClientListenSocketDisconnection;
                    connectedClient.StartListen();
                    long key = clientSocket.Handle.ToInt64();
                                      
                    IPEndPoint remoteIpEndPoint = clientSocket.RemoteEndPoint as IPEndPoint;
                    IPEndPoint localIpEndPoint = clientSocket.LocalEndPoint as IPEndPoint;

                    if (Global.Mkt.R_clients.ContainsKey(key))
                    {
                        TransactionWatch.ErrorMessage("Client with handle key '{0}' already exist!" + key);
                        return;
                    }
                    else
                    {
                        TransactionWatch.ErrorMessage("Inserting Client with handle key=" + key);
                        TransactionWatch.ErrorMessage("I am connected to " + remoteIpEndPoint.Address + " on port number " + remoteIpEndPoint.Port);
                        TransactionWatch.ErrorMessage("My local IpAddress is :" + localIpEndPoint.Address + " I am connected on port number " + localIpEndPoint.Port);
                    }
                    Global.Mkt.R_clients[key] = connectedClient;
                    r_socket.BeginAccept(new AsyncCallback(OnClientListenSocketAccept), null);
                }
                
            }
            catch (ObjectDisposedException odex)
            {
                ////Debug.Fail(odex.ToString(),
                //    "OnRMSClientConnection: Socket has been closed");
            }
            catch (Exception ex)
            {
                ////Debug.Fail(sex.ToString(),
                //    "OnRMSClientConnection: Socket failed");
            }
        }

        public byte[] StructureToByte(object packet)
        {
            try
            {
                int length = Marshal.SizeOf(packet);
                byte[] data = new byte[length];
                IntPtr intPtr = Marshal.AllocHGlobal(length);
                Marshal.StructureToPtr(packet, intPtr, true);
                Marshal.Copy(intPtr, data, 0, length);
                Marshal.FreeHGlobal(intPtr);
                return data;
            }
            catch (Exception ex)
            {
                
            }
            return null;
        }
       
        void GuiDisconnected(Socket socket)
        {
           
        }

        void GuiMessageRecived(Socket socket, byte[] message)
        {

        }

        private void OnSeverConnection(IAsyncResult asyn)
        {
            Socket clientSocket = (Socket)asyn.AsyncState;
            if (clientSocket.Connected)
            {
                clientSocket.EndConnect(asyn);
            }
            TransactionWatch.TransactionMessage("Socket connect to server : " + clientSocket.RemoteEndPoint.ToString(), Color.Blue);
            GuiRaiseClientConnected(clientSocket);
            RMSSendSocketHandler GuiconnectedClient = new RMSSendSocketHandler(clientSocket);
            GuiconnectedClient.GUIMessageRecived += OnGUIMessageRecived;
            GuiconnectedClient.Disconnected += GuiconnectedClient_Disconnected;
            GuiconnectedClient.StartListen();

            long key = clientSocket.Handle.ToInt64();
            if (Global.Mkt.m_Servers.ContainsKey(key))
            {
                ////Debug.Fail(string.Format(
                //    "Client with handle key '{0}' already exist!", key));
            }
            Global.Mkt.m_Servers[key] = GuiconnectedClient;
          
        }

        void GuiconnectedClient_Disconnected(Socket socket)
        {
            GuiRaiseClientConnected(socket);
            long key = socket.Handle.ToInt64();
            if (Global.Mkt.m_Servers.ContainsKey(key))
            {
                Global.Mkt.m_Servers.Remove(key);
            }
            else
            {
                ////Debug.Fail(string.Format(
                //    "Unknown client '{0}' has been disconncted!", key));
            }
        }

        public T PinnedPacket<T>(byte[] data)
        {
            object packet = new object();
            try
            {
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

        void OnGUIMessageRecived(Socket socket, byte[] message)
        {
            if (GUIMessageRecived != null)
            {
                BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(message);
                GUIMessageRecived(socket, message);
            }
        }

        public void GuiDistributeMessage(byte[] buffer)
        {
            try
            {
                foreach (RMSSendSocketHandler con in Global.Mkt.m_Servers.Values)
                {
                    con.Send(buffer); 
                }
            }
            catch (SocketException ex)
            {
                ////Debug.Fail(ex.ToString(), string.Format(
                //    "Buffer could not be sent"));
            }
        }

        private void OnClientListenSocketDisconnection(Socket socket)
        {

            //TransactionWatch.ErrorMessage("OnRMSClientDisconnection");
            //lock (Global._rms)
            //{
            //    TransactionWatch.ErrorMessage("OnRMSClientDisconnection Lock acquire!!!");
            //    try
            //    {
            //        long key = socket.Handle.ToInt64();
            //        TransactionWatch.ErrorMessage("Got Key!!!" + key);
            //        if (Global.Mkt.R_clients.ContainsKey(key))
            //        {
            //            long gui_id = Global.Mkt.R_gui_info[key];
            //            TransactionWatch.ErrorMessage("Removing  Gui_id  = " + gui_id);
            //            BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
            //            snd.TransCode = 17;
            //            snd.gui_id = Convert.ToUInt64(gui_id);
            //            byte[] bytesToSend = StructureToByte(snd);
            //            Global.connection1._tcpGUIPort.Send(bytesToSend);
            //            Global.Mkt.R_gui_info.Remove(key);
            //            Global.Mkt.R_clients.Remove(key);
            //            TransactionWatch.TransactionMessage("R_clients count : " + Global.Mkt.R_clients.Count, Color.Red);
            //        }
            //        RaiseRMSClientDisconnected(socket);
            //    }
            //    catch (Exception ex)
            //    {
            //        TransactionWatch.ErrorMessage("OnRMSClientDisconnection error" + ex.Message + "|" + ex.ToString());
            //    }
            //}
        }

        public void DistributeMessage(byte[] buffer)
        {
            try
            {
            }
            catch (SocketException se)
            {
                ////Debug.Fail(se.ToString(), string.Format(
                //    "Buffer could not be sent"));
            }
        }

        public void RMSDistributeMessage(byte[] buffer)
        {
            try
            {
                BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(buffer);
                if (packetHeader.TransCode == Convert.ToUInt64(Enum.Transcode.HEARTBEAT)                    
                    || packetHeader.TransCode == Convert.ToUInt64(Enum.Transcode.USER_RMS_HIT)
                    || packetHeader.TransCode == Convert.ToUInt64(Enum.Transcode.GUI_UPDATE)
                    || packetHeader.TransCode == Convert.ToUInt64(Enum.Transcode.TRADE_UPDATE)) //send to all
                {
                    if(packetHeader.TransCode == Convert.ToUInt64(Enum.Transcode.HEARTBEAT))
                    {
                        TransactionWatch.TransactionMessage("Got Heart beat from RMS | " + packetHeader.Open, Color.Blue);
                    }
                    foreach (ClientListenSocketHandler connectedClient in Global.Mkt.R_clients.Values)
                    {
                        try
                        {
                            connectedClient.Send(buffer);
                        }
                        catch (SocketException)
                        {
                            TransactionWatch.ErrorMessage("RMSDistributeMessage  ClientListenSocketHandler info not found" );
                        }
                    }
                }
                else if (packetHeader.TransCode == Convert.ToUInt64(Enum.Transcode.NEW_STRIKE_UPDATE) 
                    || packetHeader.TransCode == Convert.ToUInt64(Enum.Transcode.WIDEN_OFFSET)
                    || packetHeader.TransCode == Convert.ToUInt64(Enum.Transcode.SAVE_EOD))// dnt send anyywhere
                {
                    if (packetHeader.TransCode == Convert.ToUInt64(Enum.Transcode.NEW_STRIKE_UPDATE))
                        TransactionWatch.TransactionMessage("Transcode = " + packetHeader.TransCode + " |StrikeRequest| " + "|GuiId|"  + packetHeader.gui_id + "|UniqueId|" + packetHeader.UniqueID, Color.Blue);
                } 
                else if(packetHeader.TransCode == Convert.ToUInt64(Enum.Transcode.SEND_ADMIN_MESSAGE))
                {
                     long guiId = (long)(99);
                     long myKey = 0;
                    if (Global.Mkt.R_gui_info.ContainsValue(guiId))
                    {
                        myKey = Global.Mkt.R_gui_info.FirstOrDefault(x => x.Value == guiId).Key;
                        if (Global.Mkt.R_clients.ContainsKey(myKey) && Global.Mkt.R_gui_info.ContainsKey(myKey)) // we have to change this map
                        {
                            Global.Mkt.R_clients[myKey].Send(buffer);
                        }
                        else
                        {
                            TransactionWatch.ErrorMessage("RMSDistributeMessage|" + "guiId| " + packetHeader.gui_id + " |Key| " + myKey + " Not Present");
                        }
                    }
                }
                else if (packetHeader.TransCode == Convert.ToUInt64(Enum.Transcode.GREEK_UPDATE)
                        || packetHeader.TransCode == Convert.ToUInt64(Enum.Transcode.EOD_TRADE_TRANSMIT))
                {

                    long guiId = (long)(packetHeader.gui_id / 100000);
                    long myKey = 0;
                    if (Global.Mkt.R_gui_info.ContainsValue(guiId))
                    {
                        myKey = Global.Mkt.R_gui_info.FirstOrDefault(x => x.Value == guiId).Key;
                        if (Global.Mkt.R_clients.ContainsKey(myKey) && Global.Mkt.R_gui_info.ContainsKey(myKey)) // we have to change this map
                        {
                            Global.Mkt.R_clients[myKey].Send(buffer);
                        }
                        else
                        {
                            TransactionWatch.ErrorMessage("RMSDistributeMessage|" + "guiId| " + packetHeader.gui_id + " |Key| " + myKey + " Not Present");
                        }
                    }
                }              
                else
                {
                    try
                    {
                        long guiId = (long)(packetHeader.gui_id / 100000);
                        long myKey = 0;
                        if (Global.Mkt.R_gui_info.ContainsValue(guiId))
                        {
                            myKey = Global.Mkt.R_gui_info.FirstOrDefault(x => x.Value == guiId).Key;
                            if (Global.Mkt.R_clients.ContainsKey(myKey) && Global.Mkt.R_gui_info.ContainsKey(myKey)) // we have to change this map
                            {
                                Global.Mkt.R_clients[myKey].Send(buffer);
                            }
                            else
                            {
                                TransactionWatch.ErrorMessage("RMSDistributeMessage|" + "guiId| " + packetHeader.gui_id + " |Key| " + myKey + " Not Present");
                            }
                        }
                    }
                    catch (SocketException)
                    {
                        TransactionWatch.ErrorMessage("RMSDistributeMessage  SocketException Found");
                    }
                }
            }
            catch (SocketException se)
            {
                ////Debug.Fail(se.ToString(), string.Format(
                //    "Buffer could not be sent"));
            }
        }

        public void Close()
        {
            try
            {
                //if (m_socket != null)
                //{
                //    m_Closed = true;
                //    // Close the clients
                //    foreach (ConnectedClient connectedClient in Global.Mkt.m_clients.Values)
                //    {
                //        connectedClient.Stop();
                //    }
                //    m_socket.Close();
                //    m_socket = null;
                //}
            }
            catch (ObjectDisposedException odex)
            {
                ////Debug.Fail(odex.ToString(), "Stop failed");
            }
        }

        private void OnMessageRecived(Socket socket, byte[] message)
        {
            if (MessageRecived != null)
            {
                MessageRecived(socket, message);
            }
        }

        private void OnRMSMessageRecived(Socket socket, byte[] message)
        {
            if (RMSMessageRecived != null)
            {
                RMSMessageRecived(socket, message);
            }
        }

        private void RaiseClientConnected(Socket socket)
        {
            if (ClientConnect != null)
            {
                ClientConnect(socket);
            }
        }

        private void RaiseClientDisconnected(Socket socket)
        {
            if (ClientDisconnect != null)
            {
                ClientDisconnect(socket);
                TransactionWatch.TransactionMessage("Socket disconnect to server : " + socket.RemoteEndPoint.ToString(), Color.Blue);
                TransactionWatch.ErrorMessage("Socket disconnect to server : " + socket.RemoteEndPoint.ToString());
            }
        }

        private void GuiRaiseClientConnected(Socket socket)
        {
            if (GUIClientConnect != null)
            {
                GUIClientConnect(socket);
            }
        }

        private void GuiRaiseClientDisconnected(Socket socket)
        {
            if (GUIClientDisconnect != null)
            {
                GUIClientDisconnect(socket);
            }
        }

        private void RaiseRMSClientConnected(Socket socket)
        {
            if (RMSClientConnect != null)
            {
                RMSClientConnect(socket);
            }
        }

        private void RaiseRMSClientDisconnected(Socket socket)
        {
            if (RMSClientDisconnect != null)
            {
                RMSClientDisconnect(socket);
            }
        }
    }
}
