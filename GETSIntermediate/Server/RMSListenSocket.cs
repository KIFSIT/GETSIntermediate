using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net.Sockets;
using System.Drawing;

namespace GETSIntermediate.Server
{
    class RMSListenSocket // RMS and Intermediate Class
    {
        public byte[] left_over = new byte[160];
        int left_over_len = 0;
        public int var = 0;
        public UInt64 var1 = 0;
        public UInt64 var2 = 0;
        public UInt64 Previus = 0;
        public byte[] bytesTemp = new byte[Marshal.SizeOf(typeof(BTPacket.GUIUpdate))];
        

        public class CSocketPacket
        {
            public Socket thisSocket;
            public byte[] dataBuffer;
            public CSocketPacket(int buffeLength)
            {
                dataBuffer = new byte[buffeLength];
            }
        }
        public const  int BufferLength = 8192;
        public AsyncCallback pfnWorkerCallBack;
        Socket m_socWorker;
     
        public event RMS_MessageRecivedDel GUIMessageRecived;
        public event RMS_DisconnectDel Disconnected;

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
            catch (SocketException ex)
            {
                TransactionWatch.ErrorMessage("error WaitForData RMSListener");
            }

        }

        public void OnDataReceived(IAsyncResult asyn)
        {
            CSocketPacket theSockId = (CSocketPacket)asyn.AsyncState;
            Socket socket = theSockId.thisSocket;

            lock (Global._rms)
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
                        //Debug.Write("Apperently client has been closed and cannot answer!");
                        TransactionWatch.ErrorMessage("Apperently client has been closed and cannot answer!");
                        OnConnectionDroped(socket);
                        return;
                    }
                    if (iRx == 0)
                    {

                        TransactionWatch.ErrorMessage("Apperently client has been closed and cannot answer! RX == 0");
                        //Debug.Write("Apperently client socket has been closed.");
                        // If client socket has been closed (but client still answers)- 
                        // EndReceive will return 0.
                        OnConnectionDroped(socket);
                        return;
                    }
                    byte[] bytes1;
                    bytes1 = new byte[iRx + left_over_len];
                 
                    #region send indiviual Messages old Code 
                   /* int start_index = 0;
                    int remainingSize = bytes1.Length;
                    int end_index = bytes1.Length;
                    if (left_over_len != 0)
                    {
                        Buffer.BlockCopy(left_over, 0, bytes1, 0, left_over_len);
                    }
                    Buffer.BlockCopy(theSockId.dataBuffer, 0, bytes1, left_over_len, bytes1.Length - left_over_len);

                    while (true)
                    {
                        TransCode = BitConverter.ToUInt64(bytes1, 0);
                        if (TransCode == Convert.ToUInt64(Enum.Transcode.TRADE_UPDATE))
                        {
                            if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                            {
                                Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                left_over_len = end_index - start_index;
                                break;
                            }

                            start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                            byte[] bytesToSend = StructureToByte(packetHeader);
                            RaiseMessageRecived(bytesToSend);                           
                            remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            TransactionWatch.ErrorMessage(packetHeader.toString());
                        }
                        else if (TransCode == Convert.ToUInt64(Enum.Transcode.GUI_UPDATE))
                        {
                            if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                            {
                                
                                Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                left_over_len = end_index - start_index;
                                break;
                            }
                            start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                            byte[] bytesToSend = StructureToByte(packetHeader);
                            RaiseMessageRecived(bytesToSend);
                            remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            TransactionWatch.ErrorMessage(packetHeader.toString());
                        }
                        else if (TransCode == Convert.ToUInt64(Enum.Transcode.RESET_UPDATE))
                        {
                            if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                            {                                
                                Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                left_over_len = end_index - start_index;
                                break;
                            }
                            start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                            byte[] bytesToSend = StructureToByte(packetHeader);
                            RaiseMessageRecived(bytesToSend);
                            remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            TransactionWatch.ErrorMessage(packetHeader.toString());
                        }
                        else if (TransCode == Convert.ToUInt64(Enum.Transcode.NEW_STRIKE_UPDATE))
                        {
                            if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                            {                              
                                Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                left_over_len = end_index - start_index;
                                break;
                            }
                            start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                            byte[] bytesToSend = StructureToByte(packetHeader);
                            RaiseMessageRecived(bytesToSend);
                            remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));                            
                        }
                        else if (TransCode == Convert.ToUInt64(Enum.Transcode.EOD_TRADE_TRANSMIT))//  send netposition from server to intermediate and back to client
                        {
                            if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                            {                                
                                Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                left_over_len = end_index - start_index;
                                break;
                            }
                            start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                            byte[] bytesToSend = StructureToByte(packetHeader);                            
                            remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            TransactionWatch.ErrorMessage(packetHeader.toString());

                        }
                        else if (TransCode == Convert.ToUInt64(Enum.Transcode.ASSIGN_GUI_ID))
                        {
                            if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                            {
                                //Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                                Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                left_over_len = end_index - start_index;
                                break;
                            }
                            start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                            byte[] bytesToSend = StructureToByte(packetHeader);
                            RaiseMessageRecived(bytesToSend);

                            remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            TransactionWatch.ErrorMessage(packetHeader.toString());
                        }
                        else if (TransCode == Convert.ToUInt64(Enum.Transcode.WIDEN_OFFSET))
                        {
                            if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                            {
                                //Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                                Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                left_over_len = end_index - start_index;
                                break;
                            }
                            start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                            remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            TransactionWatch.ErrorMessage(packetHeader.toString());

                        }

                        else if (TransCode == Convert.ToUInt64(Enum.Transcode.USER_RMS_HIT))
                        {
                            if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                            {
                                //Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                                Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                left_over_len = end_index - start_index;
                                break;
                            }
                            start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                            byte[] bytesToSend = StructureToByte(packetHeader);
                            RaiseMessageRecived(bytesToSend);

                            remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            TransactionWatch.ErrorMessage(packetHeader.toString());
                            TransactionWatch.TransactionMessage("User id | " + packetHeader.Token + " Limit Hit", Color.Blue);
                            //AppGlobal.frmWatch.lblLimitHit.Text = "User id | " + packetHeader.Token + " Limit Hit";
                        }
                        else if (TransCode == Convert.ToUInt64(Enum.Transcode.RULE_NOT_FOUND))
                        {
                            if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                            {
                                //Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                                Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                left_over_len = end_index - start_index;
                                break;
                            }

                            start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);

                            byte[] bytesToSend = StructureToByte(packetHeader);
                            RaiseMessageRecived(bytesToSend);

                            remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            TransactionWatch.ErrorMessage(packetHeader.toString());
                        }
                        else if (TransCode == Convert.ToUInt64(Enum.Transcode.RULE_DELTA_REQUEST))
                        {
                            if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                            {
                                //Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                                Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                left_over_len = end_index - start_index;
                                break;
                            }

                            start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);

                            byte[] bytesToSend = StructureToByte(packetHeader);
                            RaiseMessageRecived(bytesToSend);
                            remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            TransactionWatch.ErrorMessage(packetHeader.toString());
                        }
                        else if (TransCode == Convert.ToUInt64(Enum.Transcode.HEARTBEAT))
                        {

                            if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                            {
                                //Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                                Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                left_over_len = end_index - start_index;
                                break;
                            }
                            start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                               
                            byte[] bytesToSend = StructureToByte(packetHeader);
                            RaiseMessageRecived(bytesToSend);
                            remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            TransactionWatch.ErrorMessage("Got Heart beat from RMS | " + packetHeader.Open);
                            TransactionWatch.TransactionMessage("Got Heart beat from RMS | " + packetHeader.Open, Color.Blue);
                        }
                        else
                        {
                            // TransactionWatch.TransactionMessage(start_index.ToString() + " | " + bytes1.Length, Color.Blue);
                            TransactionWatch.ErrorMessage("|NotHandledTransCode=" + TransCode);
                            if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                            {
                                //Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                                Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                left_over_len = end_index - start_index;
                                break;
                            }
                            start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));                            
                            remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        }
                        Buffer.BlockCopy(theSockId.dataBuffer, start_index - left_over_len, bytes1, 0, bytes1.Length - start_index);
                        if (start_index == end_index)
                        {
                            Array.Clear(left_over, 0, 128);
                            left_over_len = 0;
                            break;
                        }
                    }

                    */
                    #endregion

                    #region new code Send indiviual Message
                    int start_index = 0;
                    int remainingSize = bytes1.Length;
                    int end_index = bytes1.Length;

                    TransactionWatch.ErrorMessage("remainingSize|" + remainingSize + "|end_index|" + end_index + "|NewMsgLength|" + iRx);


                    if (left_over_len != 0)
                    {
                        Buffer.BlockCopy(left_over, 0, bytes1, 0, left_over_len);
                        TransactionWatch.ErrorMessage("|left_over_len|" + left_over_len);
                    }
                    Buffer.BlockCopy(theSockId.dataBuffer, 0, bytes1, left_over_len, bytes1.Length - left_over_len);
                    TransactionWatch.ErrorMessage("|left_over_len|" + left_over_len + "|bytes1.Length|" + bytes1.Length);
                   

                    while (true)
                    {
                        TransactionWatch.ErrorMessage("start|" + start_index + "|end|" + end_index);
                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                        {
                            Buffer.BlockCopy(bytes1, start_index, left_over, 0, end_index - start_index);
                            left_over_len = end_index - start_index;
                            TransactionWatch.ErrorMessage("start|" + start_index + "|end|" + end_index + "|left_over_len|" + left_over_len);
                            break;
                        }
                        Array.Clear(bytesTemp, 0, 160);
                        Buffer.BlockCopy(bytes1, start_index, bytesTemp, 0, Marshal.SizeOf(typeof(BTPacket.GUIUpdate)));
                        BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytesTemp);
                        byte[] bytesToSend = StructureToByte(packetHeader);
                        RaiseMessageRecived(bytesToSend);
                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        TransactionWatch.ErrorMessage(packetHeader.toString());
                        //Buffer.BlockCopy(theSockId.dataBuffer, start_index - left_over_len, bytes1, 0, bytes1.Length - start_index);
                        if (start_index == end_index)
                        {
                            Array.Clear(left_over, 0, 160);
                            left_over_len = 0;
                            break;
                        }
                    }
                    #endregion
                    TransactionWatch.ErrorMessage("start|" + start_index + "|end|" + end_index);
                    WaitForData(m_socWorker);
                }
                catch (Exception ex)
                {
                    TransactionWatch.ErrorMessage("Exception occured in OnDataReceived " + ex.ToString());
                    //////Debug.Fail(ex.ToString(), "OnClientConnection: Socket failed");
                }
            }
            
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

        private void RaiseMessageRecived(byte[] bytes)
        {
            if (GUIMessageRecived != null)
            {
                GUIMessageRecived(m_socWorker, bytes);
            }
        }

        private void OnDisconnection(Socket socket)
        {
            lock (Global._rms)
            {
                if (Disconnected != null)
                {
                    long key = socket.Handle.ToInt64();
                    long gui_id = Global.Mkt.R_gui_info[key];
                    TransactionWatch.ErrorMessage("Removing  Gui_id  = " + gui_id);
                    Global.Mkt.R_gui_info.Remove(key);
                    Global.Mkt.R_clients.Remove(key);
                    TransactionWatch.TransactionMessage("R_clients count : " + Global.Mkt.R_clients.Count, Color.Red);
                    TransactionWatch.ErrorMessage("OnDisconnection RMS Server and Intermediate");
                    TransactionWatch.TransactionMessage("OnDisconnection RMS Server and Intermediate",Color.Blue);
                    Disconnected(socket);
                }
            }
        }

        private void OnConnectionDroped(Socket socket)
        {
            m_socWorker = null;
            TransactionWatch.ErrorMessage("OnConnectionDroped RMS Server and Intermediate");
            TransactionWatch.TransactionMessage("OnDisconnection RMS Server and Intermediate", Color.Blue);
            OnDisconnection(socket);
        }
    }


    
}
