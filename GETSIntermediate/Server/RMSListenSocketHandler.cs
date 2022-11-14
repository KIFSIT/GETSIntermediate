using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace GETSIntermediate.Server
{
    class RMSListenSocketHandler
    {
        private Socket m_clientSocket;
        RMSListenSocket m_listener;

        public event RMS_MessageRecivedDel RMSMessageRecived
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

        public event RMS_DisconnectDel RMSClientDisconnect
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

        public RMSListenSocketHandler(Socket clientSocket)
        {
            m_clientSocket = clientSocket;
            m_listener = new RMSListenSocket();
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
