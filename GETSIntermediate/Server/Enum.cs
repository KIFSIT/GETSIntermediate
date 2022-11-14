using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GETSIntermediate.Server
{
    class Enum
    {
        public enum Market
        {
            /// <summary>
            /// Own
            /// </summary>
            Own = 0,

            /// <summary>
            /// Ncdex market
            /// </summary>
            Ncdex = 1,

            /// <summary>
            /// Mcx
            /// </summary>
            Mcx = 2,

            /// <summary>
            /// Mcx'sx
            /// </summary>
            Mcxsx = 3,

            /// <summary>
            /// 
            /// </summary>
            NseCm = 4,

            /// <summary>
            /// 
            /// </summary>
            NseFO = 5,
        }
     
        public enum Transcode : uint
        {
            DNT_USE = 0,
            GUI_UPDATE = 1,
            RESET_UPDATE = 2,
            SPREAD_BIDDING_UPDATE = 3,
            FUT_LTP_UPDATE = 4,
            TRADE_UPDATE = 5,
            SAVE_EOD = 6,
            EOD_TRADE_TRANSMIT = 7,
            MANUAL_TRADE_UPDATE = 8,
            NEW_STRIKE_UPDATE = 9,
            IMMEDIATE_ORDER_SEND = 10,
            RULE_DELTA_REQUEST = 11,
            RULE_SYN_DELTA_REQUEST = 12,
            AUTO_DELTA_REQUEST = 13,
            AUTO_SYN_DELTA_REQUEST = 14,
            DELTA_TRADE_UPDATE = 15,
            DELTA_TRADE_REJECT = 16,
            REMOVE_ALL_RULES_OF_GUI_ID = 17,
            REMOVE_RULE = 18,
            WIDEN_OFFSET = 19,
            USER_RMS_HIT = 20,
            RULE_NOT_FOUND = 21,
            HEARTBEAT = 22,            
            DUPLICATE_CLIENT = 97,
            CLIENT_CONNECT = 98,           
            ASSIGN_GUI_ID = 99,
            SEND_ADMIN_MESSAGE = 50,
            GREEK_UPDATE = 51,
            READY_TO_START = 25,
            TIMER_MSG = 45,
            PING_REPLY = 26,
            PROCESS_REPLY = 27,
            LOG_REPLY = 28,
            ADD_USER = 101,
            AddUserSuccess = 104,
            AddUserFailure=105,
            LOGIN_USER = 102,
            LOGIN_USER_SUCCESS = 103,
            LOGIN_USER_FAILURE = 112,
            MOFIFY_USER = 118,
            MODIFY_USER_SUCCESS = 119,
            MODIFY_USER_FAILURE = 120,
            LIMIT_ADD = 108,
            LIMIT_ADD_SUCCESS = 107,
            LIMIT_ADD_FAILURE =106,
            LIMIT_ADD_EXCEED = 117,
            LIMIT_CHECK=109,
            LIMIT_CHECK_ORDER_ACCOUNT_SUCCESS = 121,
            LIMIT_CHECK_ORDER_ACCOUNT_FAILURE = 122,
            LIMIT_CHECK_ORDER_USER_SUCCESS = 123,
            LIMIT_CHECK_ORDER_USER_FAILURE = 124,
            LIMIT_CHECK_SUCCESS=110,
            LIMIT_CHECK_FAILURE = 111,
            LIMIT_NOT_SET = 113,
            LIMIT_MODIFY = 114,
            LIMIT_MODIFY_SUCCESS = 115,
            LIMIT_MODIFY_FAILURE = 116
        }


        public enum EXE
        {
            IPMSG = 1,
            NOTEPAD = 2
        }

        public enum FolderName
        {
            Logs = 1
        }


    }
}
