using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GETSIntermediate;
using System.Drawing;

namespace GETSIntermediate.Server
{
    public class RMSSendSocket
    {
        
        public class CSocketPacket
        {
            
            public Socket thisSocket;
            public byte[] dataBuffer;
          
            public CSocketPacket(int buffeLength)
            {
                dataBuffer = new byte[buffeLength];
            }
        }
              
        public const int BufferLength = 200;
        public AsyncCallback pfnWorkerCallBack;
        Socket m_socWorker;
        public byte[] dataBuffer1;

        public event GUITerminal_MessageRecivedDel GUIMessageRecived;
        public event GUITerminal_DisconnectDel Disconnected;

        public void StartReciving(Socket socket)
        {
            m_socWorker = socket;
            WaitForData(socket);
        }

        public void StopListening()
        {
            // Incase connection has been established with remote client - 
            // Raise the OnDisconnection event.
            if (m_socWorker != null)
            {
                // m_socWorker.Shutdown(SocketShutdown.Both);                        
                m_socWorker.Close();
                m_socWorker = null;
            }
        }

        private void WaitForData(Socket soc)
        {
            try
            {
                if (pfnWorkerCallBack == null)
                {
                    pfnWorkerCallBack = new AsyncCallback(OnDataReceived);
                }
                CSocketPacket theSocPkt = new CSocketPacket(BufferLength);
                theSocPkt.thisSocket = soc;
               
                // now start to listen for any data...
                soc.BeginReceive(
                    theSocPkt.dataBuffer,
                    0,
                    theSocPkt.dataBuffer.Length,
                    SocketFlags.None,
                    pfnWorkerCallBack,
                    theSocPkt);
            }
            catch (SocketException sex)
            {
                ////Debug.Fail(sex.ToString(), "WaitForData: Socket failed");
            }

        }

        public void OnDataReceived(IAsyncResult asyn)
        {
            CSocketPacket theSockId = (CSocketPacket)asyn.AsyncState;
            Socket socket = theSockId.thisSocket;
            lock (Global._dataSendToClient)
            {
                if (!socket.Connected)
                {
                    return;
                }
                try
                {
                    int iRx;
                    try
                    {
                        iRx = socket.EndReceive(asyn);
                    }
                    catch (SocketException)
                    {
                        //Debug.Write("Apperently client has been closed and connot answer!");

                        OnConnectionDroped(socket);
                        return;
                    }
                    if (iRx == 0)
                    {
                        //Debug.Write("Apperently client socket has been closed.");
                        // If client socket has been closed (but client still answers)- 
                        // EndReceive will return 0.
                         OnConnectionDroped(socket);
                        return;
                    }
                    //byte[] bytes = new byte[iRx];
                    //Buffer.BlockCopy(theSockId.dataBuffer, 0, bytes, 0, iRx);
                    //Global.connection1._tcpGUIPort.Send(bytes);

                    byte[] bytes = new byte[iRx];
                    //Buffer.BlockCopy(theSockId.dataBuffer, 0, bytes, 0, iRx);
                    //Global.connection1._tcpGUIPort.Send(theSockId.dataBuffer);
                    Global.connection1.tcpClient.Client.Send(theSockId.dataBuffer);
                    WaitForData(m_socWorker);
                }
                catch (Exception ex)
                {
                   
                }
            }
        }

        private void RaiseMessageRecived(byte[] bytes)
        {
            if (GUIMessageRecived != null)
            {
                GUIMessageRecived(m_socWorker, bytes);
            }
        }

        private void OnDisconnection(Socket socket)
        {
            lock (Global._dataSendToClient)
            {
                if (Disconnected != null)
                {
                    Disconnected(socket);
                    long key = socket.Handle.ToInt64();
                    Global.Mkt.m_Servers.Remove(key);
                    TransactionWatch.TransactionMessage("m_Server count : " + Global.Mkt.m_Servers.Count, Color.Red);
                }
            }
        }

        private void OnConnectionDroped(Socket socket)
        {
            m_socWorker = null;
            OnDisconnection(socket);

        }
    }
}
