using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GETSIntermediate.AllClasses
{
    public class SqlUserTable
    {
        public static string UserId = "Id";
        public static string GroupId = "GroupId";
        public static string Category = "Category";
        public static string ClientName = "ClientName";
        public static string Password = "Password";
        public static string BranchName = "BranchName";
        public static string PanNo = "PanNo";
        public static string MappedTo = "MappedTo";
        public static string Ip = "IpAddress";
        public static string UserStatus = "UserStatus";
        public static string City = "City";
        public static string PinCode = "PinCode";
        public static string IsActive = "IsActive";
        public static string ExpiryDate = "ExpiryDate";
    }

    public class Users
    {
        public static string UserId = "id";
        public static string GroupId = "group_id";
        public static string Category = "category";
        public static string ClientName = "name";
        public static string Password = "password";
        public static string BranchName = "branch_name";
        public static string PanNo = "pan_no";
        public static string MappedTo = "mapped_to";
        public static string Ip = "ip";
        public static string UserStatus = "status";
        public static string City = "city";
        public static string PinCode = "pin_code";
        public static string ExpiryDate = "expiry";
        public static string IsActive = "is_active";
       
    }

    public class Accounts
    {
        public static string Id = "id";
        public static string Category = "category";
        public static string ClientName = "name";
        public static string Password = "password";
        public static string BranchName = "branch_name";
        public static string PanNo = "pan_no";
        public static string MappedTo = "mapped_to";
        public static string Ip = "ip";
        public static string UserStatus = "status";
        public static string City = "city";
        public static string PinCode = "expiry";
        public static string IsActive = "is_active";
        public static string ExpiryDate = "pin_code";
    }

    public class LimitDetails
    {
        public static string Id = "id";
        public static string SingleOrderLotSize = "single_order_lot";
        public static string SingleOrderValue = "single_order_value";
        public static string BuyQty = "buy_qty";
        public static string SellQty = "sell_qty";
        public static string NetQty = "net_qty";
        public static string BuyLimit = "buy_limit";
        public static string SellLimit = "sell_limit";
        public static string SprdLotSize = "spread_lot";
        public static string SprdOrderValue = "spread_value";
        public static string GroupId = "group_id";
    }
}
