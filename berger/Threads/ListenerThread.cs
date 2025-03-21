using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;

namespace berger.Threads
{
    public class ListenerThread
    {
        private Thread thread;
        private TcpListener listener;
        private int serverPort = 8888;
        private int connectedClientsCounter = 0;
        public bool mainServer = false;

    public ListenerThread(bool mainServer)
        {
            this.mainServer = mainServer;
            thread = new Thread(new ParameterizedThreadStart(threadTask));
        }
        public void start()
        {
            thread.Start();
        }
        public void stop()
        {
            listener.Stop();
            foreach (var item in ClientThreadManager.handleClientList)
            {
                item.stop();
            }
            ClientThreadManager.handleClientList.Clear();
        }
        private void threadTask(object obj)
        {
            try
            {
                if(mainServer != true)
                {
                    serverPort = 0;
                }
                listener = new TcpListener(IPAddress.Any, serverPort);
                listener.Start();
                Debug.WriteLine($"Serwer {(mainServer == true ? "nadzorujący" : "")} uruchomiony. Oczekiwanie na połączenia...");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    HandleClientThread handleClientThread = new HandleClientThread();
                    ClientThreadManager.handleClientList.Add(handleClientThread);
                    handleClientThread.start(client);
                    connectedClientsCounter++;
                    Debug.WriteLine($"Podłączono serwer...");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }
    }
}
