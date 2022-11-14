using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace GETSIntermediate.Server
{
    public class BTPacket
    {
        public struct MessageHeader
        {
            public UInt64 TransCode;
        }
            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            public struct TradeMessage
            {
                public UInt64 TransCode;
                public UInt64 Token;
                public UInt64 ExchangeSequence;
                public UInt64 Sequence;
                public double Price;
                public int Qty;
                public int Side; // 66 buy 83 sell
              
            }

            //[StructLayout(LayoutKind.Sequential, Pack = 32)]
            //public struct UserDetails
            //{
            //    //[MarshalAs(UnmanagedType.SysUInt, SizeConst = 120)]
            //    //[FieldOffset(0)]
            //    public  UInt64 TransCode;
            //    public int isActive;
            //    public int uniqueId;
            //    public int pinCode;

            //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            //    public string tableName;

            //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            //    public string expiryDate;

            //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            //    public string groupId;

            //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            //    public string category;

            //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            //    public string clientName;

            //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            //    public string password;

            //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            //    public string branchName;

            //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            //    public string panNo;

            //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            //    public string mappedTo;

            //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            //    public string ipAddress;

            //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            //    public string userStatus;

            //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            //    public string userId;

            //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            //    public string city;
            //}

            [StructLayout(LayoutKind.Sequential, Pack = 32)]
            public struct UserDetails
            {
                //[MarshalAs(UnmanagedType.SysUInt, SizeConst = 120)]
                //[FieldOffset(0)]
                public UInt64 TransCode;
                public int is_active;
                public int uniqueId;
                public int pin_code;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string tableName;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string expiry;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string group_id;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string category;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string name;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string password;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string branch_name;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string pan_no;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string mapped_to;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string ip;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string status;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string id;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string city;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            public struct LimitCheck
            {
                    public UInt64 TransCode;
                    public int uniqueId;
                    public int OrderId;
                    public int qty;
                    public int lot;
                    public double Limit;

                    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                    public string id;

                    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                    public string group_id;

                    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                    public string Strategy;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            public struct LimitConfirmation
            {
                public UInt64 TransCode;
                public int uniqueId;
                public bool isSuccess;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            public struct AddUserConfirmation
            {
                public UInt64 TransCode;
                public int uniqueId;
                public bool IsSuccess;
            }


            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            public struct LoginUser
            {
                public UInt64 TransCode;
                public int uniqueId;

                public int Password;
                public bool IsSuccess;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string Id;
            }


            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            public struct LoginConfirmation
            {
                public UInt64 TransCode;
                public int uniqueId;
                public bool IsSuccess;

            }

            // GUI update 1 
            // Reset update 2
            // L4_LC_LP_UC_UP_SpreadBiddingUpdate 3
            // FUT ltp update 4
            // TrdUpdate3L 5
            // Save_EOD 6
            // EOD trade Retransfer 7
            // Manual Trade entry 8
            // RMS Error Message 9
            // Trade Massage 10
            // VOL_update 11
            // TWO_Leg_Trade_Update 12 // TradePrice = Leg1 Price and Transcost  = Leg2 Price
            // request_new_strike 13
            // new_strike_request 9
            // Intermediate_order_send 10
            // winden_offset 19
            // Rule_Not_Found 21
            // Immidiate_send_Order 10

            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            public struct GUIUpdate
            {
                public UInt64 TransCode; 
                public double Wind;
                public double Unwind;
                public double AvgSpread;
                public double Netting;
                public double TradePrice;
                public double TransactionCost;
                public UInt64 gui_id;
                public UInt64 UniqueID;
                public UInt64 StrategyId;//3434
                public int Token;
                public int Open;
                public int Round;
                public int WindPos;
                public int UnWindPos;
                public int OverNightWindPos;
                public int OverNightUnWindPos;
                public bool IsbeenTraded;
                public int isWind;
                public int rollWind;
                public int rollUNWind;
                public int autoTraderWind;
                public int autoTraderUNWind;
                public int autoSqroff;
                public int autoSqroffhit;
                public int bidding_wind_offset;
                public int bidding_unwind_offset;
                public int autoTraderWindOffset;
                public int autoTraderUNWindOffset;
                public int Machine_ip;
                public string toString()
                {
                    string ss = "";
                    switch (TransCode)
                    {
                        case (ulong)Enum.Transcode.GUI_UPDATE:
                            ss = ss + "GUI|Enter" + UniqueID + "|strategyID|" + StrategyId + "|ask_wind|" + Wind + "|bid_unwind|" + Unwind + "|open|" + Open + "|round|" + Round + "|wind|" + WindPos + "|unwind|" + UnWindPos + "|avgspread|" + AvgSpread;
                            break;
                        case (ulong)Enum.Transcode.RESET_UPDATE:
                            ss = ss + "GUI|Reset" + UniqueID + "|strategyID|" + StrategyId + "|ask_wind|" + Wind + "|bid_unwind|" + Unwind + "|open|" + Open + "|round|" + Round + "|wind|" + WindPos + "|unwind|" + UnWindPos + "|avgspread|" + AvgSpread;
                            break;
                        case (ulong)Enum.Transcode.TRADE_UPDATE:
                            string wind = "";
                        if (isWind == 1)
                            wind = "Wind";
                        else
                            wind = "Unwind";   
                            ss = ss + "TRADE|Fut_Token|" + Token +"|Uniq_id|"+ UniqueID + "|strategyID|" + StrategyId + "|gui_id|" + gui_id + "|tradePrice|" + TradePrice + "|txncost|" + TransactionCost + "|open|" + Open + "|round|" + Round + "|wind|" + WindPos + "|unwind|" + UnWindPos + "|avgspread|" + AvgSpread + "|Iswind|" + wind;
                            break;
                        case (ulong)Enum.Transcode.NEW_STRIKE_UPDATE:
                            ss = ss + "STRIKEREQUEST|Fut_Token" + Token + "|Uniq_id|" + UniqueID + "|strategyID|" + StrategyId + "|gui_id|" + gui_id + "|Token1|" + Open + "|Token2|" + Round + "|Token3|" + WindPos + "|Token4|" + UnWindPos;
                            break;
                        case (ulong)Enum.Transcode.IMMEDIATE_ORDER_SEND:
                            ss = ss + "IMMEDIATE_ORDER_SEND" + "|GUI|" + gui_id + "|TransCode|" + TransCode;
                            break;
                        case (ulong)Enum.Transcode.DELTA_TRADE_REJECT:
                            ss = ss + "DELTA_TRADE_REJECT" + "|GUI|" + gui_id + "|TransCode|" + TransCode;
                            break;
                        case (ulong)Enum.Transcode.REMOVE_ALL_RULES_OF_GUI_ID:
                            ss = ss + "REMOVE_ALL_RULES_OF_GUI_ID" + "|GUI|" + gui_id + "|TransCode|" + TransCode;
                            break;
                        case (ulong)Enum.Transcode.REMOVE_RULE:
                            break;
                        case (ulong)Enum.Transcode.WIDEN_OFFSET:
                            ss = ss + "WIDEN_OFFSET|" + "|GUI|" + gui_id + "|TransCode|" + TransCode;
                            break;
                        case (ulong)Enum.Transcode.USER_RMS_HIT:
                            ss = ss + "USER_RMS_HIT|" + "TransCode|" + TransCode + "|Limit Hit|" + Token;
                            break;
                        case (ulong)Enum.Transcode.RULE_NOT_FOUND:
                            ss = ss + "RULE_NOT_FOUND|" + "GUI|" + gui_id + "|TransCode| " + TransCode + "|Rule_Found|" + UniqueID;
                            break;
                        case (ulong)Enum.Transcode.HEARTBEAT:
                            ss = ss + "HEARTBEAT|" + "TransCode| " + TransCode + "|Got Heart beat from RMS|" + Open;

                            break;
                        case (ulong)Enum.Transcode.EOD_TRADE_TRANSMIT:
                            ss = "EOD_TRADE_TRANSMIT|" + UniqueID + "|windpos|" + (WindPos + OverNightWindPos) + "|unwindpos|" + (UnWindPos + OverNightUnWindPos) + "|av. spread|" + AvgSpread + "|netting|" + Netting + "|fut_price|" + autoTraderWindOffset;
                            break;

                        default:
                            break;
                    }
                    return ss;
                }

                

            }

            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            public struct ADD_CLIENT
            {
                public UInt64 TransCode;
                public int ClientId;
                public UInt64 UniqueID;
            }


            //[StructLayout(LayoutKind.Sequential, Pack = 4)]
            //public struct LimitDetails
            //{
            //    public UInt64 Transcode;
            //    public int UniqueID;
            //    public double singleOrderValue;
            //    public double spreadOrderValue;
            //    public int singleOrderLotSize;
            //    public int buyQty;
            //    public int sellQty;
            //    public int netQty;

            //    public int spreadOrderLotSize;
            //    public int BuyLimit;
            //    public int SellLimit;

            //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            //    public string TableName;

            //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            //    public string Id;

            //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            //    public string GroupId;
            //}

            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            public struct LimitDetails1
            {
                public UInt64 Transcode;
                public int UniqueID;
                public double single_order_value {get; set;}
                public double spread_value { get; set; }
                public int single_order_lot { get; set; }
                public int buy_qty { get; set; }
                public int sell_qty { get; set; }
                public int net_qty { get; set; }

                public int spread_lot { get; set; }
                public int buy_limit { get; set; }
                public int sell_limit { get; set; }

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string table_name;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string id;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string group_id;
            }


            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            public struct LimitDetails
            {
                public UInt64 Transcode;
                public int UniqueID;
                public double single_order_value;
                public double spread_value;
                public int single_order_lot;
                public int buy_qty;
                public int sell_qty;
                public int net_qty;

                public int spread_lot;
                public int buy_limit;
                public int sell_limit;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string table_name;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string id;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
                public string group_id;
            }

    }
}
