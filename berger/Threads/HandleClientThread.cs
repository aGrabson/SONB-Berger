using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace berger.Threads
{
    public class HandleClientThread
    {
        private Thread thread;
        private TcpClient tcpClient;
        public NetworkStream stream;
        public string clientIP;
        public int clientPort;
        public HandleClientThread()
        {
            thread = new Thread(new ParameterizedThreadStart(threadTask));
        }
        public void start(TcpClient client)
        {
            thread.Start(client);
        }
        public void stop()
        {
            tcpClient.Close();
            stream.Close();
        }
        private void threadTask(object obj)
        {
            //bool isClientEnd = false;
            //tcpClient = (TcpClient)obj;
            //stream = tcpClient.GetStream();

            //clientIP = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();
            //clientPort = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port;
            //var newClientRecord = new CliendRecord { IPAddress = clientIP, Port = clientPort, Count = 0 };
            //Application.Current.Dispatcher.Invoke(() =>
            //{
            //    ///ClientListPage.ConnectedClientsRecords.Add(newClientRecord);
            //    ClientListPage.addClient(newClientRecord);
            //});

            //try
            //{
            //    while (true)
            //    {
            //        int bufferSize = 1024;
            //        int initBufferSize = bufferSize;
            //        byte[] tmpResponseData = new byte[bufferSize];
            //        int bytesRead = 0;
            //        int totalBytesRead = 0;

            //        // Odczytywanie odpowiedzi
            //        while (true)
            //        {
            //            isClientEnd = handleClientDisconect(tcpClient, stream, newClientRecord, clientIP, clientPort);
            //            if (isClientEnd)
            //                break;

            //            bytesRead = stream.Read(tmpResponseData, totalBytesRead, bufferSize);
            //            totalBytesRead += bytesRead;
            //            string tmpS = Encoding.UTF8.GetString(tmpResponseData);
            //            if (tmpS.Contains("END"))
            //            {
            //                break;
            //            }
            //            initBufferSize += bufferSize;
            //            Array.Resize(ref tmpResponseData, initBufferSize);
            //        }

            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //    //MessageBox.Show(ex.ToString(), "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            //    Application.Current.Dispatcher.Invoke(() =>
            //    {
            //        ClientListPage.deleteClient(clientIP, clientPort);
            //    });
            //}
            //finally
            //{
            //    stream.Close();
            //    tcpClient.Close();
            //}
        }
    }
}
