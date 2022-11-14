using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using ArisDev;
using GETSIntermediate;
using System.Diagnostics;


namespace GETSIntermediate.Server
{
    /// <summary>
    /// 
    /// </summary>
    public static class TransactionWatch
    {
        #region Method

        private delegate void MsgData(string msg, Color color);

        public static void ErrorMessage(string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    StackFrame stackFrame = new StackFrame(1, true);
                    int line = stackFrame.GetFileLineNumber();
                    string filename = stackFrame.GetFileName();

                    message = filename.Split('\\').Last() + "|" + line + "|" + message;
                    Global.frmWatch.WriteToErrorLog(message);
                }
            }
            catch (Exception) { }
        }

        public static void TransactionMessage(string message, Color color)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    Message(message, color);
                }
            }
            catch (Exception) { }
        }

        private static void Message(string message, Color color)
        {
            try
            {
                if (Global.frmWatch.tbDebugLog.InvokeRequired)
                {
                    MsgData obj = Message;
                    Global.frmWatch.tbDebugLog.Invoke(obj, new object[] { message, color });
                }
                else if (Global.frmWatch.tbDebugLog != null)
                {
                    if (Global.frmWatch.tbDebugLog.Text.Length > 50000)
                    {
                        var lines = Global.frmWatch.tbDebugLog.Lines;
                        int numOfLines = lines.ToArray().Length - 5;
                        var newLines = lines.Skip(numOfLines);
                        Global.frmWatch.tbDebugLog.Lines = newLines.ToArray();
                    }

                    Global.frmWatch.tbDebugLog.SelectionStart = Global.frmWatch.tbDebugLog.Text.Length;
                    Global.frmWatch.tbDebugLog.SelectionColor = color;
                    Global.frmWatch.tbDebugLog.SelectedText = DateTime.Now.ToString("HH:mm:ss:ffff >> ") + message + Environment.NewLine;
                    Global.frmWatch.tbDebugLog.ScrollToCaret();
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }
}
