using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using GETSIntermediate;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Data.SqlServerCe;
using GETSIntermediate.Server;
using System.Drawing;
namespace GETSIntermediate
{
    public class SqlConnectionLocal
    {
        SqlCeConnection conn;
        public SqlCeConnection Connect()
        {
            //Data Source=D:\GETSIntermediate\GETSIntermediate\LoginDetails.sdf
            conn = new SqlCeConnection(@"Data Source=D:\GETSIntermediate\GETSIntermediate\GetsData.sdf");//config
            conn.Open();
            Console.WriteLine("Connected to User Database.");
            return conn;
        }

        public bool Add(string tableName, string columnNames,string values)
        {
            bool isAdded=false;
            try
            {
                
               // string query = "INSERT INTO UserDetails (UserId,City) VALUES('101','Mumbai')";
                string query = "INSERT INTO "+tableName + " (" + columnNames + ")" + " VALUES(" + values+ ")";
                using (SqlCeCommand myCmd = new SqlCeCommand(query, conn))
                {
                    myCmd.CommandTimeout = 0;
                    myCmd.CommandType = CommandType.Text;
                    myCmd.ExecuteNonQuery();
                    isAdded = true;
                }
               // MessageBox.Show("Data added successfully.");
            }
            catch (SqlCeException ex)
            {
                if (ex.NativeError == 25016)
                {
                    TransactionWatch.TransactionMessage("Cannot add data because UserId already exists.", Color.Red);
                }
                //else throw;
            }
            catch (Exception ex)
            {
                TransactionWatch.TransactionMessage("An error occured while adding data to table." + ex.ToString(), Color.Red);
            }
            return isAdded;
        }



        public string ReadDataUsingWhere(string tableName, string resultCol, string columnName, string value)
        {
            string result = "";
            try
            {
               
                string query = "SELECT " + resultCol + " FROM " + tableName + " WHERE " + columnName + "='" + value + "'";
                //string query = " SELECT * FROM " + ArisApi_a._arisApi.SystemConfig.Test_TableName + " where tick_timestamp ='" + "1305278100" + "'";//1305278100,1305191700
                SqlCeCommand cmd = new SqlCeCommand(query, conn);
                cmd.CommandTimeout = 0;
                //SqlCeDataReader mysqlreader = cmd.ExecuteReader();

                SqlCeDataReader mysqlreader = cmd.ExecuteResultSet(ResultSetOptions.Scrollable);
                //if (mysqlreader.HasRows)
                {
                    while (mysqlreader.Read())//4
                    {
                        result = mysqlreader[resultCol].ToString();
                    }
                }
                //else
                //{
                //    result = "0";
                //}
            }
            catch (Exception ex)
            { 
            
            }
            return result;
        }


        public int PerformOperations(string tableName, string resultCol, string columnName, string value,string operation)
        {
            //SELECT SUM(column_name) FROM table_name WHERE condition;
            int result = 0;
            string query = "SELECT " + operation + "(" + resultCol + ")"+  " FROM " + tableName + " WHERE " + columnName + "='" + value + "'";
            SqlCeCommand cmd = new SqlCeCommand(query, conn);
            cmd.CommandTimeout = 0;
           // SqlCeDataReader mysqlreader = cmd.ExecuteResultSet(ResultSetOptions.Scrollable);
            if (cmd.ExecuteScalar() != DBNull.Value)
            {
                result = Convert.ToInt32((cmd.ExecuteScalar()));

            }

            //while (mysqlreader.Read())//4
            //{
            //    if (mysqlreader.HasRows)
            //    {
            //        result = Convert.ToInt32(mysqlreader[resultCol]);
            //    }
            //    else
            //    {
            //        result = 0;
            //    }
            //}
            return result;
        }

        public int PerformOperationsExceptValue(string tableName, string resultCol, string columnName, string value, string operation, string excCol, string excVal)
        {
            //SELECT SUM(column_name) FROM table_name WHERE condition;
            int result = 0;
            string query = "SELECT " + operation + "(" + resultCol + ")" + " FROM " + tableName + " WHERE " + columnName + "='" + value + "'" + " AND " + excCol + "<>'" + excVal + "'";
            SqlCeCommand cmd = new SqlCeCommand(query, conn);
            cmd.CommandTimeout = 0;
            // SqlCeDataReader mysqlreader = cmd.ExecuteResultSet(ResultSetOptions.Scrollable);
            if (cmd.ExecuteScalar() != DBNull.Value)
            {
                result = Convert.ToInt32((cmd.ExecuteScalar()));

            }

            //while (mysqlreader.Read())//4
            //{
            //    if (mysqlreader.HasRows)
            //    {
            //        result = Convert.ToInt32(mysqlreader[resultCol]);
            //    }
            //    else
            //    {
            //        result = 0;
            //    }
            //}
            return result;
        }

        public SqlCeDataReader ReadRecordUsingWhere(string tableName, string resultCol, string columnName, string value)
        {
            SqlCeDataReader mysqlreader;
            //string result = "";
            string query = "SELECT " + resultCol + " FROM " + tableName + " WHERE " + columnName + "='" + value + "'";
            //string query = " SELECT * FROM " + ArisApi_a._arisApi.SystemConfig.Test_TableName + " where tick_timestamp ='" + "1305278100" + "'";//1305278100,1305191700
            SqlCeCommand cmd = new SqlCeCommand(query, conn);
            cmd.CommandTimeout = 0;
            mysqlreader = cmd.ExecuteReader();
            return mysqlreader;
        }

        public bool UpdateRecord(string tableName, string query, string whereCol, string whereValue)
        {
            bool isModified = false;
            try
            {
                string updateQuery = "UPDATE " + tableName + " SET " + query + " WHERE " + whereCol + "='" + whereValue + "'";
                using (SqlCeCommand myCmd = new SqlCeCommand(updateQuery, conn))
                {
                    myCmd.CommandTimeout = 0;
                    myCmd.CommandType = CommandType.Text;
                    myCmd.ExecuteNonQuery();
                    isModified = true;
                }
                //MessageBox.Show("Data updated successfully for user " + whereValue);
                
            }
            catch (Exception)
            {

                //throw;
            }
            return isModified;
        }

        public SqlCeDataReader ReadAllData(string tableName)
        {
            string query = "SELECT * FROM " + tableName;
            //string query = " SELECT * FROM " + ArisApi_a._arisApi.SystemConfig.Test_TableName + " where tick_timestamp ='" + "1305278100" + "'";//1305278100,1305191700
            SqlCeCommand cmd = new SqlCeCommand(query, conn);
            cmd.CommandTimeout = 0;
            SqlCeDataReader mysqlreader = cmd.ExecuteReader();
          
            //while (mysqlreader.Read())//4
            //{
            //     string oneRowData ="";
            //     string temp ="";
            //    for (int i = 0; i < mysqlreader.FieldCount; i++)
            //    {
            //        temp = mysqlreader[i].ToString();
            //        if (oneRowData == "")
            //        {
            //            oneRowData = temp;
            //        }
            //        else
            //        {
            //            oneRowData = oneRowData + "," + temp;
            //        }
                   
            //    }
            //    Console.WriteLine(oneRowData);
            //    //Console.WriteLine(mysqlreader[SqlUserTable.UserId] + "," + mysqlreader[SqlUserTable.GroupId] + "," + mysqlreader[SqlUserTable.ClientName] + ","
            //    //    + mysqlreader[SqlUserTable.City] + "," + mysqlreader[SqlUserTable.Category] + "," + mysqlreader[SqlUserTable.Ip] + "," + mysqlreader[SqlUserTable.BranchName] + "," + mysqlreader[SqlUserTable.ExpiryDate]);
            //}
            return mysqlreader;
        }
    }
}
