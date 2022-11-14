using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace GETSIntermediate.Server
{
    public class ClientListenSocketHandler
    {
         private Socket m_clientSocket;
         ClientListenSocket m_listener;
         public string ClientIp;
        public event RMSTerminal_MessageRecivedDel MessageRecived
        {
            add
            {
                m_listener.MessageRecived += value;
            }
            remove
            {
                m_listener.MessageRecived -= value;
            }
        }

        public event RMSTerminal_DisconnectDel Disconnected
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

        public ClientListenSocketHandler(Socket clientSocket)
        {
            m_clientSocket = clientSocket;
            m_listener = new ClientListenSocket();
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
