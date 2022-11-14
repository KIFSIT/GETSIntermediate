using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using GETSIntermediate.AllClasses;
using System.Reflection;
using System.Data.SqlServerCe;
using System.Collections;

namespace GETSIntermediate.Server
{
    class ClientListenSocket // Client and Intermediate Class
    {
        public byte[] left_over = new byte[256];
        int left_over_len = 0;
        public class CSocketPacket
        {
            public Socket thisSocket;
            public byte[] dataBuffer;
            public CSocketPacket(int buffeLength)
            {
                dataBuffer = new byte[buffeLength];
            }
        }

        private const int BufferLength = 1400;
        AsyncCallback pfnWorkerCallBack;
        Socket m_socWorker;


        public event RMSTerminal_MessageRecivedDel MessageRecived;
        public event RMSTerminal_DisconnectDel Disconnected;

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
                TransactionWatch.TransactionMessage("WaitForData: SocketException", Color.Blue);
                TransactionWatch.ErrorMessage("WaitForData : SocketException");
                OnConnectionDroped(soc);
            }
        }

        private void OnDataReceived(IAsyncResult asyn)
        {
            CSocketPacket theSockId = (CSocketPacket)asyn.AsyncState;
            Socket socket = theSockId.thisSocket;

            if (!socket.Connected)
            {
                return;
            }

            try
            {
                lock (Global._rms)
                {
                    int iRx;
                    try
                    {
                        iRx = socket.EndReceive(asyn);
                    }
                    catch (SocketException)
                    {
                        //Debug.Write("Apperently client has been closed and connot answer.");
                        TransactionWatch.TransactionMessage("SocketException", Color.Blue);
                        TransactionWatch.ErrorMessage("SocketException");
                        OnConnectionDroped(socket);
                        return;
                    }

                    if (iRx == 0)
                    {
                        // Debug.Write("Apperently client socket has been closed.");
                        // If client socket has been closed (but client still answers) - 
                        // EndReceive will return 0.
                        TransactionWatch.TransactionMessage("SocketException RX == 0", Color.Blue);
                        TransactionWatch.ErrorMessage("iRx == 0");
                        OnConnectionDroped(socket);
                        return;
                    }

                    if (iRx != 0)
                    {
                        byte[] bytes1;
                        UInt64 TransCode = 0;
                        bytes1 = new byte[iRx + left_over_len];

                        int start_index = 0;
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
                            if (TransCode == Convert.ToUInt64(Enum.Transcode.CLIENT_CONNECT)) // Client Connected Message
                            {

                                if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.ADD_CLIENT)))
                                {
                                    Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                    left_over_len = end_index - start_index;
                                    break;
                                }
                                BTPacket.ADD_CLIENT packetHeader = PinnedPacket<BTPacket.ADD_CLIENT>(bytes1);
                                long key = socket.Handle.ToInt64();
                                if (!Global.Mkt.R_gui_info.ContainsValue(packetHeader.ClientId))
                                {
                                    Global.Mkt.R_gui_info.Add(key, (long)(packetHeader.ClientId));// GUI ID
                                    BTPacket.ADD_CLIENT snd = new BTPacket.ADD_CLIENT();
                                    snd.TransCode = 99;
                                    snd.UniqueID = Convert.ToUInt64(packetHeader.ClientId) * 100000;
                                    byte[] bytesToSend = StructureToByte(snd);
                                    Global.Mkt.R_clients[key].Send(bytesToSend);
                                    TransactionWatch.ErrorMessage("Assigning new Gui_id " + snd.UniqueID);
                                    TransactionWatch.ErrorMessage("Key|" + key + "|GUI id |" + packetHeader.ClientId);
                                }
                                else
                                {
                                    Global.Gui_Flg = true;
                                    BTPacket.ADD_CLIENT snd = new BTPacket.ADD_CLIENT();
                                    snd.TransCode = 99;
                                    snd.UniqueID = 0; // return 0 as gui id
                                    byte[] bytesToSend = StructureToByte(snd);
                                    Global.Mkt.R_clients[key].Send(bytesToSend);
                                    Global.Mkt.R_clients.Remove(key);
                                    TransactionWatch.ErrorMessage(" new Gui_id not Assigning " + snd.UniqueID + " |key|" + key + "|GuiID|" + packetHeader.ClientId);
                                }
                                start_index = start_index + Marshal.SizeOf(typeof(BTPacket.ADD_CLIENT));
                            }
                            else if (TransCode == Convert.ToUInt64(Enum.Transcode.DUPLICATE_CLIENT)) // when duplicate send closed
                            {
                                //long key = socket.Handle.ToInt64();
                                //if (Global.Mkt.R_clients.ContainsKey(key))
                                //{
                                //    long gui_id = Global.Mkt.R_gui_info[key];
                                //    TransactionWatch.ErrorMessage("Removing  Gui_id  = " + gui_id);
                                //    Global.Mkt.R_gui_info.Remove(key);
                                //    Global.Mkt.R_clients.Remove(key);
                                //    TransactionWatch.TransactionMessage("R_clients count : " + Global.Mkt.R_clients.Count + "duplicate ID", Color.Red);
                                //}
                                //else
                                //{
                                //}
                                if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                                {
                                    Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                    left_over_len = end_index - start_index;
                                    break;
                                }

                                BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                                TransactionWatch.TransactionMessage("R_clients count : duplicate ID Closed |" + packetHeader.gui_id, Color.Red);
                                start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                            }
                            else if (TransCode == Convert.ToUInt64(Enum.Transcode.REMOVE_ALL_RULES_OF_GUI_ID)) // Client Closing  Message
                            {
                                if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                                {
                                    Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                    left_over_len = end_index - start_index;
                                    break;
                                }
                                start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                                BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                                if (Global.Mkt.R_gui_info.ContainsValue(packetHeader.WindPos))
                                {

                                    TransactionWatch.TransactionMessage("Remove Original Gui No " + packetHeader.WindPos, Color.Blue);
                                    //Global.Gui_no.Remove(packetHeader.WindPos);
                                    TransactionWatch.TransactionMessage("R_Server count : " + Global.Mkt.R_clients.Count, Color.Red);
                                    TransactionWatch.ErrorMessage("Remove Original Gui No " + packetHeader.WindPos);
                                }
                                byte[] bytesToSend = StructureToByte(packetHeader);
                                //Global.connection1._tcpGUIPort.Send(bytesToSend);
                                Global.connection1.tcpClient.Client.Send(bytesToSend);
                            }
                            else if (TransCode == Convert.ToUInt64(Enum.Transcode.ADD_USER))
                            {
                                if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.UserDetails)))
                                {
                                    Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                    left_over_len = end_index - start_index;
                                    break;
                                }

                                BTPacket.UserDetails user = fromBytes(bytes1);
                                string dt = Convert.ToDateTime(user.expiry).ToString("yyyyMMdd");
                                string values = "";
                                string columnNames = "";
                                bool isAdded;
                                string printMsg = "";
                                if (user.tableName == "Accounts")
                                {
                                    columnNames = Accounts.Id + "," + Accounts.Category + "," + Accounts.ClientName + "," + Accounts.Password + ","
                                    + Accounts.BranchName + "," + Accounts.PanNo + "," + Accounts.MappedTo + "," + Accounts.Ip + ","
                                    + Accounts.UserStatus + "," + Accounts.City + "," + Accounts.ExpiryDate + "," + Accounts.IsActive + ","
                                    + Accounts.PinCode;

                                    values = "'" + user.group_id + "','" + user.category + "','" + user.name + "','" + user.password + "','" + user.branch_name
+ "','" + user.pan_no + "','" + user.mapped_to + "','" + user.ip + "','" + user.status + "','" + user.city
+ "','" + dt + "'," + user.is_active + "," + user.pin_code;
                                    isAdded = Global.sqlConnectionObj.Add(user.tableName, columnNames, values);
                                    printMsg = "Acoount with Id " + user.group_id + " created successfully";
                                }
                                else
                                {
                                    columnNames = Users.UserId + "," + Users.Password + "," + Users.GroupId + "," + Users.Category + ","
                                         + Users.ClientName + "," + Users.BranchName + "," + Users.PanNo + "," + Users.MappedTo + ","
                                         + Users.Ip + "," + Users.UserStatus + "," + Users.City + "," + Users.PinCode + ","
                                         + Users.ExpiryDate + "," + Users.IsActive;

                                    values = "'" + user.id + "','" + user.password + "','" + user.group_id + "','" + user.category + "','" + user.name
                                   + "','" + user.branch_name + "','" + user.pan_no + "','" + user.mapped_to + "','" + user.ip + "','" + user.status
                                   + "','" + user.city + "'," + user.pin_code + ",'" + dt + "'," + user.is_active;
                                    isAdded = Global.sqlConnectionObj.Add(user.tableName, columnNames, values);
                                    printMsg = "User with Id " + user.id + " added successfully to account " + user.group_id;
                                }

                                GETSIntermediate.Server.BTPacket.MessageHeader msg = new BTPacket.MessageHeader();
                                if (isAdded)
                                {
                                    msg.TransCode = Convert.ToUInt64(Enum.Transcode.AddUserSuccess);
                                    TransactionWatch.TransactionMessage(printMsg, Color.Blue);
                                }
                                else
                                {
                                    msg.TransCode = Convert.ToUInt64(Enum.Transcode.AddUserFailure);
                                    TransactionWatch.TransactionMessage("Error occured while adding User with Id " + user.id + ".", Color.Blue);
                                }
                                byte[] bytesToSend = StructureToByte(msg);
                                SendReplyToClient(user.uniqueId, bytesToSend);
                                start_index = start_index + Marshal.SizeOf(typeof(BTPacket.UserDetails));
                            }
                            //else if (TransCode == Convert.ToUInt64(Enum.Transcode.LIMIT_ADD))
                            //{
                            //    if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.LimitDetails)))
                            //    {
                            //        Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                            //        left_over_len = end_index - start_index;
                            //        break;
                            //    }
                            //    BTPacket.LimitDetails addLimit = fromBytesAddLimit(bytes1);
                            //    #region SQL
                            //    string columnNames = ""; string values = "";
                            //    GETSIntermediate.Server.BTPacket.MessageHeader msg = new BTPacket.MessageHeader();
                            //    if (addLimit.table_name == "User_Limit")
                            //    {
                            //        int storedAccountLimit = Convert.ToInt32(Global.sqlConnectionObj.ReadDataUsingWhere("Account_Limit", "single_order_value", "id", addLimit.group_id));
                            //        int totalUsersLimit = Global.sqlConnectionObj.PerformOperations("User_Limit", "single_order_value", "group_id", addLimit.group_id, "SUM");
                            //        int canAddNumber = storedAccountLimit - totalUsersLimit;
                            //        if (totalUsersLimit != 0)
                            //        {
                            //            if (addLimit.single_order_value > canAddNumber)
                            //            {
                            //                //if (totalUsersLimit > storedAccountLimit)
                            //                {
                            //                    msg.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_ADD_EXCEED);
                            //                    TransactionWatch.TransactionMessage("Cannot set limit for user " + addLimit.id + ", account limit already full", Color.Red);
                            //                    goto skip;
                            //                }
                            //            }
                            //        }

                            //        columnNames = LimitDetails.Id + "," + LimitDetails.SingleOrderLotSize + "," + LimitDetails.SingleOrderValue + "," + LimitDetails.BuyQty + ","
                            //                          + LimitDetails.SellQty + "," + LimitDetails.NetQty + "," + LimitDetails.BuyLimit + "," + LimitDetails.SellLimit + "," +
                            //                          LimitDetails.SprdLotSize + "," + LimitDetails.SprdOrderValue + "," + LimitDetails.GroupId;

                            //        values = "'" + addLimit.id + "'," + addLimit.single_order_lot + "," + addLimit.single_order_lot + "," + addLimit.buy_qty + "," + addLimit.sell_qty
                            //        + "," + addLimit.net_qty + "," + addLimit.buy_limit + "," + addLimit.sell_limit + "," + addLimit.spread_lot + "," + addLimit.spread_value + ",'" + addLimit.group_id + "'";
                            //    }
                            //    else
                            //    {
                            //        columnNames = LimitDetails.Id + "," + LimitDetails.SingleOrderLotSize + "," + LimitDetails.SingleOrderValue + "," + LimitDetails.BuyQty + ","
                            //                             + LimitDetails.SellQty + "," + LimitDetails.NetQty + "," + LimitDetails.BuyLimit + "," + LimitDetails.SellLimit + "," +
                            //                             LimitDetails.SprdLotSize + "," + LimitDetails.SprdOrderValue;

                            //        values = "'" + addLimit.id + "'," + addLimit.single_order_lot + "," + addLimit.single_order_lot + "," + addLimit.buy_qty + "," + addLimit.sell_qty
                            // + "," + addLimit.net_qty + "," + addLimit.buy_limit + "," + addLimit.sell_limit + "," + addLimit.spread_lot + "," + addLimit.spread_value;
                            //    }

                            //    bool isAdded = Global.sqlConnectionObj.Add(addLimit.table_name, columnNames, values); 
                            //    #endregion

                            //    string print = "";
                            //    if (addLimit.table_name == "Account_Limit")
                            //    {
                            //        print = "Account";
                            //    }
                            //    else
                            //    {
                            //        print = "User";
                            //    }
                            //    //GETSIntermediate.Server.BTPacket.MessageHeader msg = new BTPacket.MessageHeader();

                            //    #region ReplyMsg
                            //    if (isAdded)
                            //    {
                            //        msg.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_ADD_SUCCESS);
                            //        TransactionWatch.TransactionMessage("Limits set successfully for " + print + " " + addLimit.id, Color.Blue);
                            //    }
                            //    else
                            //    {
                            //        msg.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_ADD_FAILURE);
                            //        TransactionWatch.TransactionMessage("Limits couldn't set successfully for " + print + " " + addLimit.id, Color.Red);
                            //    }
                            //skip:
                            //    byte[] bytesToSend = StructureToByte(msg);
                            //    foreach (ClientListenSocketHandler connectedClient in Global.Mkt.R_clients.Values)
                            //    {
                            //        try
                            //        {
                            //            var myKey = Global.Mkt.R_clients.FirstOrDefault(x => x.Value == connectedClient).Key;
                            //            long id = Global.Mkt.R_gui_info[myKey];
                            //            if (addLimit.UniqueID == id)
                            //            {
                            //                connectedClient.Send(bytesToSend);
                            //            }
                            //        }
                            //        catch (SocketException)
                            //        {
                            //            TransactionWatch.ErrorMessage("RMSDistributeMessage  ClientListenSocketHandler info not found");
                            //        }
                            //    } 
                            //    #endregion

                            //    start_index = start_index + Marshal.SizeOf(typeof(BTPacket.LimitDetails));
                            //}
                            else if (TransCode == Convert.ToUInt64(Enum.Transcode.LIMIT_CHECK))
                            {
                                if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.LimitCheck)))
                                {
                                    Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                    left_over_len = end_index - start_index;
                                    break;
                                }
                                BTPacket.LimitCheck addLimit = PinnedPacket<BTPacket.LimitCheck>(bytes1);
                                GETSIntermediate.Server.BTPacket.MessageHeader msgAcc = new BTPacket.MessageHeader();
                                GETSIntermediate.Server.BTPacket.MessageHeader msgUser = new BTPacket.MessageHeader();
                            
                                string isPresentAcc = Global.sqlConnectionObj.ReadDataUsingWhere("Account_Limit", "id", "id", addLimit.group_id.ToString());
                                if (isPresentAcc == "")
                                {
                                    TransactionWatch.TransactionMessage("Set Limits for account and user to place the order.",Color.Red);
                                    msgAcc.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_NOT_SET);
                                    byte[] bytesToSend = StructureToByte(msgAcc);
                                    SendReplyToClient(addLimit.uniqueId, bytesToSend);
                                    
                                    start_index = start_index + Marshal.SizeOf(typeof(BTPacket.LimitCheck));
                                    goto End;
                                    //return;
                                }

                                string isPresentUser = Global.sqlConnectionObj.ReadDataUsingWhere("User_Limit", "id", "id", addLimit.id.ToString());
                                if (isPresentUser == "")
                                {
                                    TransactionWatch.TransactionMessage("Set Limits for user to place the order.", Color.Red);
                                    msgUser.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_NOT_SET);
                                    byte[] bytesToSend = StructureToByte(msgUser);
                                    SendReplyToClient(addLimit.uniqueId, bytesToSend);
                                    
                                    start_index = start_index + Marshal.SizeOf(typeof(BTPacket.LimitCheck));
                                    goto End;
                                    //return;
                                }

                                //account
                                //order value
                                #region order value
                                string storedLimitForAccount = "";  
                                if (addLimit.Strategy == "Single")
                                {
                                    storedLimitForAccount = Global.sqlConnectionObj.ReadDataUsingWhere("Account_Limit", "single_order_value", "id", addLimit.group_id.ToString());

                                }
                                else
                                {
                                    storedLimitForAccount = Global.sqlConnectionObj.ReadDataUsingWhere("Account_Limit", "spread_value", "id", addLimit.group_id.ToString());
                                }


                                if (storedLimitForAccount == "")
                                {
                                    msgAcc.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_NOT_SET);
                                    TransactionWatch.TransactionMessage("Order value limits are not set for account " + addLimit.group_id, Color.Red);
                                    byte[] bytesToSend = StructureToByte(msgAcc);
                                    SendReplyToClient(addLimit.uniqueId, bytesToSend);
                                    
                                }
                                else
                                {
                                    if (addLimit.Limit > Convert.ToInt32(storedLimitForAccount))
                                    {
                                        TransactionWatch.TransactionMessage("Order Value Limit Hit on account level for user " + addLimit.id + " belongs to " + addLimit.group_id, Color.Red);
                                        msgAcc.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_CHECK_ORDER_ACCOUNT_FAILURE);
                                        byte[] bytesToSend = StructureToByte(msgAcc);
                                        SendReplyToClient(addLimit.uniqueId, bytesToSend);
                                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.LimitCheck));
                                        goto End;
                                       // return;
                                    }
                                } 
                                #endregion

                                #region Lot
                                string storedLotAcc = "";
                                if (addLimit.Strategy == "Single")
                                {
                                    storedLotAcc = Global.sqlConnectionObj.ReadDataUsingWhere("Account_Limit", "single_order_lot", "id", addLimit.group_id.ToString());

                                }
                                else
                                {
                                    storedLotAcc = Global.sqlConnectionObj.ReadDataUsingWhere("Account_Limit", "spread_lot", "id", addLimit.group_id.ToString());
                                }

                                if (storedLotAcc == "")
                                {
                                    msgAcc.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_NOT_SET);
                                    TransactionWatch.TransactionMessage("Lot limits are not set for account " + addLimit.group_id, Color.Red);
                                    byte[] bytesToSend = StructureToByte(msgAcc);
                                    SendReplyToClient(addLimit.uniqueId, bytesToSend);
                                    start_index = start_index + Marshal.SizeOf(typeof(BTPacket.LimitCheck));
                                    //return;
                                    goto End;
                                }
                                else
                                {
                                    if (addLimit.lot > Convert.ToInt32(storedLotAcc))
                                    {
                                        TransactionWatch.TransactionMessage("Lot Limit Hit on account level for user " + addLimit.id + " belongs to " + addLimit.group_id, Color.Red);
                                        msgAcc.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_CHECK_ORDER_ACCOUNT_FAILURE);
                                        byte[] bytesToSend = StructureToByte(msgAcc);
                                        SendReplyToClient(addLimit.uniqueId, bytesToSend);
                                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.LimitCheck));
                                        goto End;
                                    }
                                }  
                                #endregion

                                #region Qty
                                string storedQtyAcc = "";
                               storedQtyAcc = Global.sqlConnectionObj.ReadDataUsingWhere("Account_Limit", "net_qty", "id", addLimit.group_id.ToString());

                               if (storedQtyAcc == "")
                                {
                                    msgAcc.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_NOT_SET);
                                    TransactionWatch.TransactionMessage("Net Quantity limits are not set for account " + addLimit.group_id, Color.Red);
                                    byte[] bytesToSend = StructureToByte(msgAcc);
                                    SendReplyToClient(addLimit.uniqueId, bytesToSend);
                                    goto End;
                                    //return;
                                }
                                else
                                {
                                    if (addLimit.qty > Convert.ToInt32(storedQtyAcc))
                                    {
                                        TransactionWatch.TransactionMessage("Net Qty Limit Hit on account level for user " + addLimit.id + " belongs to " + addLimit.group_id, Color.Red);
                                        msgAcc.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_CHECK_ORDER_ACCOUNT_FAILURE);
                                        byte[] bytesToSend = StructureToByte(msgAcc);
                                        SendReplyToClient(addLimit.uniqueId, bytesToSend);
                                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.LimitCheck));
                                        goto End;
                                       // return;
                                    }
                                }
                                #endregion

                                //user

                               #region Order Value
                               string storedLimit = "";
                               if (addLimit.Strategy == "Single")
                               {
                                   storedLimit = Global.sqlConnectionObj.ReadDataUsingWhere("User_Limit", "single_order_value", "id", addLimit.id.ToString());
                               }
                               else
                               {
                                   storedLimit = Global.sqlConnectionObj.ReadDataUsingWhere("User_Limit", "spread_value", "id", addLimit.id.ToString());
                               }
                               if (storedLimit == "")
                               {
                                   msgUser.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_NOT_SET);
                                   TransactionWatch.TransactionMessage("Order value limits are not set for UserId " + addLimit.id, Color.Red);
                                   byte[] bytesToSend = StructureToByte(msgUser);
                                   SendReplyToClient(addLimit.uniqueId, bytesToSend);
                                   start_index = start_index + Marshal.SizeOf(typeof(BTPacket.LimitCheck));
                                   goto End;
                                  // return;
                               }
                               else
                               {
                                   if (Convert.ToDouble(storedLimit) < addLimit.Limit)
                                   {
                                       msgUser.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_CHECK_ORDER_USER_FAILURE);
                                       TransactionWatch.TransactionMessage("Order Value Limit Hit on user level for user " + addLimit.id, Color.Red);
                                       byte[] bytesToSend = StructureToByte(msgUser);
                                       SendReplyToClient(addLimit.uniqueId, bytesToSend);
                                       start_index = start_index + Marshal.SizeOf(typeof(BTPacket.LimitCheck));
                                       goto End;
                                   }
                               } 
                               #endregion

                               #region Lot
                               string storedLimitLot = "";
                               if (addLimit.Strategy == "Single")
                               {
                                   storedLimitLot = Global.sqlConnectionObj.ReadDataUsingWhere("User_Limit", "single_order_lot", "id", addLimit.id.ToString());
                               }
                               else
                               {
                                   storedLimitLot = Global.sqlConnectionObj.ReadDataUsingWhere("User_Limit", "spread_lot", "id", addLimit.id.ToString());
                               }
                               if (storedLimitLot == "")
                               {
                                   msgUser.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_NOT_SET);
                                   TransactionWatch.TransactionMessage("Lots Limits are not set for UserId " + addLimit.id, Color.Red);
                                   byte[] bytesToSend = StructureToByte(msgUser);
                                   SendReplyToClient(addLimit.uniqueId, bytesToSend);
                                   //return;
                                   start_index = start_index + Marshal.SizeOf(typeof(BTPacket.LimitCheck));
                                   goto End;
                               }
                               else
                               {
                                   if (Convert.ToDouble(storedLimitLot) < addLimit.lot)
                                   {
                                       msgUser.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_CHECK_ORDER_USER_FAILURE);
                                       TransactionWatch.TransactionMessage("Lot Limit Hit on user level for user " + addLimit.id, Color.Red);
                                       byte[] bytesToSend = StructureToByte(msgUser);
                                       SendReplyToClient(addLimit.uniqueId, bytesToSend);
                                       start_index = start_index + Marshal.SizeOf(typeof(BTPacket.LimitCheck));
                                       goto End;
                                   }
                               } 
                               #endregion

                               #region Qty
                               string storedLimitQty = "";
                             
                               storedLimitQty = Global.sqlConnectionObj.ReadDataUsingWhere("User_Limit", "net_qty", "id", addLimit.id.ToString());
                               if (storedLimitQty == "")
                               {
                                   msgUser.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_NOT_SET);
                                   TransactionWatch.TransactionMessage("Order value Limits are not set for UserId " + addLimit.id, Color.Red);
                                   byte[] bytesToSend = StructureToByte(msgUser);
                                   SendReplyToClient(addLimit.uniqueId, bytesToSend);
                                   start_index = start_index + Marshal.SizeOf(typeof(BTPacket.LimitCheck));
                                   goto End;
                                   //return;
                               }
                               else
                               {
                                   if (Convert.ToDouble(storedLimitQty) < addLimit.qty)
                                   {
                                       msgUser.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_CHECK_ORDER_USER_FAILURE);
                                       TransactionWatch.TransactionMessage("Net Qty Limit Hit on user level for user " + addLimit.id, Color.Red);
                                       byte[] bytesToSend = StructureToByte(msgUser);
                                       SendReplyToClient(addLimit.uniqueId, bytesToSend);
                                       start_index = start_index + Marshal.SizeOf(typeof(BTPacket.LimitCheck));
                                       goto End;
                                   }
                               } 


                               #endregion
                               msgUser.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_CHECK_ORDER_USER_SUCCESS);
                               TransactionWatch.TransactionMessage("Order placed successfully for user " + addLimit.id, Color.Blue);
                               byte[] bytesToSend1 = StructureToByte(msgUser);
                               SendReplyToClient(addLimit.uniqueId, bytesToSend1);
                               start_index = start_index + Marshal.SizeOf(typeof(BTPacket.LimitCheck));
                            }
                            else if (TransCode == Convert.ToUInt64(Enum.Transcode.LOGIN_USER))
                            {
                                if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.LoginUser)))
                                {
                                    Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                    left_over_len = end_index - start_index;
                                    break;
                                }
                                BTPacket.LoginUser msg = fromBytesLogin(bytes1);
                                //  BTPacket.LoginUser msg = PinnedPacket<BTPacket.LoginUser>(bytes1);
                                string storedPass = Global.sqlConnectionObj.ReadDataUsingWhere("Users", "password", "id", msg.Id.ToString());
                                GETSIntermediate.Server.BTPacket.MessageHeader msg1 = new BTPacket.MessageHeader();
                                if (storedPass != "")
                                {
                                    if (storedPass == msg.Password.ToString())
                                    {
                                        msg1.TransCode = Convert.ToUInt64(Enum.Transcode.LOGIN_USER_SUCCESS);
                                    }
                                    else
                                    {
                                        msg1.TransCode = Convert.ToUInt64(Enum.Transcode.LOGIN_USER_FAILURE);
                                    }
                                }
                                byte[] bytesToSend = StructureToByte(msg1);
                                SendReplyToClient(msg.uniqueId, bytesToSend);
                                start_index = start_index + Marshal.SizeOf(typeof(BTPacket.LoginUser));
                            }
                            else if (TransCode == Convert.ToUInt64(Enum.Transcode.MOFIFY_USER))
                            {
                                if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.UserDetails)))
                                {
                                    Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                    left_over_len = end_index - start_index;
                                    break;
                                }
                                BTPacket.UserDetails msg = fromBytes(bytes1);
                                GETSIntermediate.Server.BTPacket.MessageHeader msg1 = new BTPacket.MessageHeader();
                                //query
                                string query = CheckIfBlank(msg);
                                bool isModified = Global.sqlConnectionObj.UpdateRecord("Users", query, "id", msg.id);
                                if (isModified)
                                {
                                    msg1.TransCode = Convert.ToUInt64(Enum.Transcode.MODIFY_USER_SUCCESS);
                                    TransactionWatch.TransactionMessage("Data updated successfully for user " + msg.id, Color.Blue);
                                }
                                else
                                {
                                    msg1.TransCode = Convert.ToUInt64(Enum.Transcode.MODIFY_USER_FAILURE);
                                    TransactionWatch.TransactionMessage("An error occured while updating data for user " + msg.id, Color.Red);
                                }
                                byte[] bytesToSend = StructureToByte(msg1);
                                SendReplyToClient(msg.uniqueId, bytesToSend);
                                start_index = start_index + Marshal.SizeOf(typeof(BTPacket.UserDetails));
                            }
                            else if (TransCode == Convert.ToUInt64(Enum.Transcode.LIMIT_ADD))
                            {
                                if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.LimitDetails)))
                                {
                                    Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                    left_over_len = end_index - start_index;
                                    break;
                                }
                                BTPacket.LimitDetails addLimit = fromBytesAddLimit(bytes1);
                                string columnNames = ""; string values = "";

                                GETSIntermediate.Server.BTPacket.MessageHeader msg = new BTPacket.MessageHeader();

                                if (addLimit.table_name == "User_Limit")
                                {
                                    SqlCeDataReader reader = Global.sqlConnectionObj.ReadRecordUsingWhere("Account_Limit", "*", "id", addLimit.group_id);
                                    List<string> listCol = new List<string>();
                                    List<string> listVal = new List<string>();
                                    Type type = typeof(BTPacket.LimitDetails);
                                    var fields = type.GetFields();//bt
                                    foreach (FieldInfo field in fields)
                                    {
                                        if (field.Name != "Transcode" && field.Name != "UniqueID" && field.Name != "table_name" && field.Name != "id" && field.Name != "group_id")
                                        {
                                            var val = addLimit.GetType().GetField(field.Name).GetValue(addLimit);
                                            if (val != "0")
                                            {
                                                int storedAccountLimit = Convert.ToInt32(Global.sqlConnectionObj.ReadDataUsingWhere("Account_Limit", field.Name, "id", addLimit.group_id));
                                                if (storedAccountLimit == 0)
                                                {
                                                    TransactionWatch.TransactionMessage("Set account limits first for account: " + addLimit.group_id, Color.Red);
                                                    start_index = start_index + Marshal.SizeOf(typeof(BTPacket.LimitDetails));
                                                    goto End;
                                                }
                                                int totalUsersLimit = Global.sqlConnectionObj.PerformOperations("User_Limit", field.Name, "group_id", addLimit.group_id, "SUM");
                                                int canAddNumber = (storedAccountLimit - totalUsersLimit);
                                                //if (totalUsersLimit >= storedAccountLimit)
                                                {
                                                    if (Convert.ToInt32(val) > canAddNumber)
                                                    {
                                                        msg.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_ADD_EXCEED);
                                                        TransactionWatch.TransactionMessage("Cannot set limit for user " + addLimit.id + ", account limit already full for " + field.Name, Color.Red);
                                                        //start_index = start_index + Marshal.SizeOf(typeof(BTPacket.LimitDetails));
                                                        //goto End;
                                                    }
                                                    else
                                                    {
                                                        listCol.Add(field.Name);
                                                        listVal.Add(val.ToString());
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    columnNames = string.Join(",", listCol) + "," + LimitDetails.GroupId + "," + LimitDetails.Id;
                                    values = string.Join(",", listVal) + ",'" + addLimit.group_id + "','" + addLimit.id + "'";
                                }
                                else
                                {
                                    columnNames = LimitDetails.Id + "," + LimitDetails.SingleOrderLotSize + "," + LimitDetails.SingleOrderValue + "," + LimitDetails.BuyQty + ","
                                                         + LimitDetails.SellQty + "," + LimitDetails.NetQty + "," + LimitDetails.BuyLimit + "," + LimitDetails.SellLimit + "," +
                                                         LimitDetails.SprdLotSize + "," + LimitDetails.SprdOrderValue;

                                    values = "'" + addLimit.id + "'," + addLimit.single_order_lot + "," + addLimit.single_order_lot + "," + addLimit.buy_qty + "," + addLimit.sell_qty
                             + "," + addLimit.net_qty + "," + addLimit.buy_limit + "," + addLimit.sell_limit + "," + addLimit.spread_lot + "," + addLimit.spread_value;
                                }

                                bool isAdded = Global.sqlConnectionObj.Add(addLimit.table_name, columnNames, values);
                                string print = "";
                                if (addLimit.table_name == "Account_Limit")
                                {
                                    print = "Account";
                                }
                                else
                                {
                                    print = "User";
                                }
                                #region ReplyMsg
                                if (isAdded)
                                {
                                    msg.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_ADD_SUCCESS);
                                    TransactionWatch.TransactionMessage("Limits set successfully for " + print + " " + addLimit.id, Color.Blue);
                                }
                                else
                                {
                                    msg.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_ADD_FAILURE);
                                    TransactionWatch.TransactionMessage("Limits couldn't set successfully for " + print + " " + addLimit.id, Color.Red);
                                }

                                byte[] bytesToSend = StructureToByte(msg);
                                SendReplyToClient(addLimit.UniqueID, bytesToSend);
                                #endregion
                                start_index = start_index + Marshal.SizeOf(typeof(BTPacket.LimitDetails));
                            }
                            else if (TransCode == Convert.ToUInt64(Enum.Transcode.LIMIT_MODIFY))
                            {
                                if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.LimitDetails)))
                                {
                                    Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                                    left_over_len = end_index - start_index;
                                    break;
                                }
                                BTPacket.LimitDetails modifyLimit = fromBytesAddLimit(bytes1);
                                GETSIntermediate.Server.BTPacket.MessageHeader msg = new BTPacket.MessageHeader();
                                List<string> list = new List<string>();
                                if (modifyLimit.table_name == "User_Limit")
                                {
                                    Type type = typeof(BTPacket.LimitDetails);
                                    var fields = type.GetFields();//bt
                                    foreach (FieldInfo field in fields)
                                    {
                                        if (field.Name != "Transcode" && field.Name != "UniqueID" && field.Name != "table_name" && field.Name != "id" && field.Name != "group_id")
                                        {
                                            var val = modifyLimit.GetType().GetField(field.Name).GetValue(modifyLimit);
                                            if (val != "0")
                                            {
                                                int storedAccountLimit = Convert.ToInt32(Global.sqlConnectionObj.ReadDataUsingWhere("Account_Limit", field.Name, "id", modifyLimit.group_id));
                                           //     int totalUsersLimit = Global.sqlConnectionObj.PerformOperations("User_Limit", field.Name, "group_id", modifyLimit.group_id, "SUM");
                                                int totalUsersLimit = Global.sqlConnectionObj.PerformOperationsExceptValue("User_Limit", field.Name, "group_id", modifyLimit.group_id, "SUM" , "id" , modifyLimit.id);
                                                int canAddNumber = (storedAccountLimit - totalUsersLimit);
                                                //if (totalUsersLimit >= storedAccountLimit)
                                                {
                                                    if (Convert.ToInt32(val) > canAddNumber)
                                                    {
                                                        list.Add(field.Name);
                                                        msg.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_ADD_EXCEED);
                                                        TransactionWatch.TransactionMessage("Cannot set limit for user " + modifyLimit.id + ", account limit already full for " + field.Name, Color.Red);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    string query = CheckIfBlankLimit(modifyLimit, list);
                                    if (query != "")
                                    {
                                        string commaseperatedstring = string.Join(",", Global.updatedColumn);
                                        bool isModified = Global.sqlConnectionObj.UpdateRecord("User_Limit", query, "id", modifyLimit.id);
                                        string print = "";

                                        if (isModified)
                                        {
                                            msg.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_MODIFY_SUCCESS);
                                            TransactionWatch.TransactionMessage("Limit updated successfully for " + print + " " + modifyLimit.id + " for " + commaseperatedstring, Color.Blue);
                                        }
                                        else
                                        {
                                            msg.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_MODIFY_FAILURE);
                                            TransactionWatch.TransactionMessage("An error occured while updating limit for " + print + " " + modifyLimit.id, Color.Red);
                                        }
                                    }
                                }
                                else
                                {
                                    string query = CheckIfBlankLimit(modifyLimit, list);
                                    if (query != "")
                                    {
                                        string commaseperatedstring = string.Join(",", Global.updatedColumn);
                                        bool isModified = Global.sqlConnectionObj.UpdateRecord("Account_Limit", query, "id", modifyLimit.id);
                                        string print = "";

                                        if (isModified)
                                        {
                                            msg.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_MODIFY_SUCCESS);
                                            TransactionWatch.TransactionMessage("Limit updated successfully for " + print + " " + modifyLimit.id + " for " + commaseperatedstring, Color.Blue);
                                        }
                                        else
                                        {
                                            msg.TransCode = Convert.ToUInt64(Enum.Transcode.LIMIT_MODIFY_FAILURE);
                                            TransactionWatch.TransactionMessage("An error occured while updating limit for " + print + " " + modifyLimit.id, Color.Red);
                                        }
                                    }
                                }

                            skip:
                                byte[] bytesToSend = StructureToByte(msg);
                                SendReplyToClient(modifyLimit.UniqueID, bytesToSend);
                                start_index = start_index + Marshal.SizeOf(typeof(BTPacket.LimitDetails));
                            }
                            else
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
                                //Global.connection1._tcpGUIPort.Send(bytesToSend);
                                //Global.connection1.tcpClient.Client.Send(bytesToSend);
                            }
                        End:
                            Buffer.BlockCopy(theSockId.dataBuffer, start_index - left_over_len, bytes1, 0, bytes1.Length - start_index);
                            if (start_index == end_index)
                            {
                                Array.Clear(left_over, 0, 128);
                                left_over_len = 0;
                                break;
                            }
                        }

                        #region Original Code
                        //byte[] bytes = new byte[iRx];
                        //Buffer.BlockCopy(theSockId.dataBuffer, 0, bytes, 0, iRx);
                        //Global.connection1._tcpGUIPort.Send(bytes);
                        //RaiseMessageRecived(bytes) 
                        #endregion
                    }
                    WaitForData(m_socWorker);
                }
            }
            catch (Exception ex)
            {
                TransactionWatch.ErrorMessage("OnDataReceived Exception" + ex.Message + "|" + ex.ToString());
            }
        }

        public string CheckIfBlank(object obj)
        {
            string query = "";
            string temp = "";
            Type type = typeof(BTPacket.UserDetails);
            var fields = type.GetFields();//bt
            foreach (FieldInfo field in fields)
            {
                if (field.Name != "tableName" && field.Name != "TransCode" && field.Name != "uniqueId")
                {
                    var fieldValue = field.GetValue(obj);//(string)field.GetValue(null);
                    if (fieldValue.ToString() != "0" && fieldValue.ToString() != "" && fieldValue.ToString() != "-1")
                    {

                        temp = field.Name + "='" + fieldValue.ToString() + "'";
                        if (query != "")
                        {
                            query = query + "," + temp;
                        }
                        else
                        {
                            query = temp;
                        }
                    }
                }
            }
            return query;
        }


        public string CheckIfBlankLimit(object obj,List<string> list)
        {
            Global.updatedColumn.Clear();
            string query = "";
            string temp = "";
            Type type = typeof(BTPacket.LimitDetails);
            var fields = type.GetFields();//bt
            foreach (FieldInfo field in fields)
            {
                if (field.Name != "table_name" && field.Name != "Transcode" && field.Name != "UniqueID" && !list.Contains(field.Name) && field.Name != "group_id" && field.Name != "id")
                {
                    var fieldValue = field.GetValue(obj);//(string)field.GetValue(null);
                    if (fieldValue.ToString() != "0" && fieldValue.ToString() != "")
                    {
                        Global.updatedColumn.Add(field.Name);
                        temp = field.Name + "='" + fieldValue.ToString() + "'";
                        if (query != "")
                        {
                            query = query + "," + temp;
                        }
                        else
                        {
                            query = temp;
                        }
                    }
                }
            }
            return query;
        }

        public static void SendReplyToClient(long userUniqueId, byte[] msgToSend)
        {

            foreach (ClientListenSocketHandler connectedClient in Global.Mkt.R_clients.Values)
            {
                try
                {
                    var myKey = Global.Mkt.R_clients.FirstOrDefault(x => x.Value == connectedClient).Key;
                    long id = Global.Mkt.R_gui_info[myKey];
                    if (id == userUniqueId)
                    {
                        connectedClient.Send(msgToSend);
                    }
                }
                catch (SocketException)
                {
                    TransactionWatch.ErrorMessage("RMSDistributeMessage  ClientListenSocketHandler info not found");
                }
            }
        }

        internal static byte[] StructureToByte(object packet)
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
            catch (Exception)
            {
            }
            return null;
        }

        public T PinnedPacket<T>(byte[] data)
        {
            object packet = new object();
            try
            {
                //int length = Marshal.SizeOf(data);
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

        BTPacket.UserDetails fromBytes(byte[] arr)
        {
            BTPacket.UserDetails str = new BTPacket.UserDetails();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);

                Marshal.Copy(arr, 0, ptr, size);

                str = (BTPacket.UserDetails)Marshal.PtrToStructure(ptr, str.GetType());
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return str;
        }

        BTPacket.LimitDetails fromBytesAddLimit(byte[] arr)
        {
            BTPacket.LimitDetails str = new BTPacket.LimitDetails();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);

                Marshal.Copy(arr, 0, ptr, size);

                str = (BTPacket.LimitDetails)Marshal.PtrToStructure(ptr, str.GetType());
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return str;
        }

        BTPacket.LoginUser fromBytesLogin(byte[] arr)
        {
            BTPacket.LoginUser str = new BTPacket.LoginUser();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);

                Marshal.Copy(arr, 0, ptr, size);

                str = (BTPacket.LoginUser)Marshal.PtrToStructure(ptr, str.GetType());
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return str;
        }

        private void RaiseMessageRecived(byte[] bytes)
        {
            if (MessageRecived != null)
            {
                MessageRecived(m_socWorker, bytes);
            }
        }

        private void OnDisconnection(Socket socket)
        {
            TransactionWatch.ErrorMessage("Entering OnDisconnection");
            lock (Global._rms)
            {
                
                TransactionWatch.ErrorMessage("Lock acquire...");
                if (Disconnected != null)
                {
                    
                    long key = socket.Handle.ToInt64();
                    TransactionWatch.ErrorMessage("Removing key  = " +key) ;
                    if (Global.Mkt.R_clients.ContainsKey(key) && Global.Mkt.R_gui_info.ContainsKey(key))
                    {
                        long gui_id = Global.Mkt.R_gui_info[key];
                        TransactionWatch.ErrorMessage("Removing  Gui_id  = " + gui_id + "|Key|" + key);
                        Global.Mkt.R_gui_info.Remove(key);
                        Global.Mkt.R_clients.Remove(key);
                        Global.Gui_no.Remove(Convert.ToInt32(gui_id));
                        BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                        snd.TransCode = 17;
                        snd.gui_id = Convert.ToUInt64(gui_id * 100000);
                        byte[] bytesToSend = StructureToByte(snd);
                        //Global.connection1._tcpGUIPort.Send(bytesToSend);
                        Global.connection1.tcpClient.Client.Send(bytesToSend);
                        TransactionWatch.TransactionMessage("R_Server count : " + Global.Mkt.R_clients.Count, Color.Red);
                        TransactionWatch.ErrorMessage("OnDisconnection Client and Intermediate|" + gui_id);
                        TransactionWatch.TransactionMessage("OnDisconnection Client and Intermediate" + gui_id, Color.Blue);
                        Disconnected(socket);
                    }
                    else
                    {
                        TransactionWatch.ErrorMessage("Not Present in Client  map  = " + key);
                    }
                }
            }
        }

        private void OnConnectionDroped(Socket socket)
        {
            TransactionWatch.ErrorMessage("OnConnectionDroped ");
            TransactionWatch.ErrorMessage("OnConnectionDroped Client and Intermediate|");
            TransactionWatch.TransactionMessage("OnConnectionDroped Client and Intermediate", Color.Blue);
            m_socWorker = null;
            OnDisconnection(socket);
        }
    }
}
