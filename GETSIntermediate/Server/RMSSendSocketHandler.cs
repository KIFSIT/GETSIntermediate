using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using GETSIntermediate.Server;

namespace GETSIntermediate.Server
{
    public class RMSSendSocketHandler
    {
        private Socket m_clientSocket;
        RMSSendSocket m_listener;

        public event GUITerminal_MessageRecivedDel GUIMessageRecived
        {
            add
            {
                m_listener.GUIMessageRecived += value;
            }

            remove
            {
                m_listener.GUIMessageRecived -= value;
            }
        }

        public event GUITerminal_DisconnectDel Disconnected
        {
            add
            {
                m_listener.Disconnected += value;
            }

            remove
            {
                m_listener.Disconnected -= value;
            }
        }

        public RMSSendSocketHandler(Socket clientSocket)
        {
            m_clientSocket = clientSocket;

            m_listener = new RMSSendSocket();
        }
       
        public void StartListen()
        {
            m_listener.StartReciving(m_clientSocket);
        }

        public void Send(byte[] buffer)
        {
            if (m_clientSocket == null)
            {
                throw new Exception("Can't send data. ConnectedClient is Closed!");
            }
            m_clientSocket.Send(buffer);

        }

        public void Stop()
        {
            m_listener.StopListening();
            m_clientSocket = null;
        }
    }
}
