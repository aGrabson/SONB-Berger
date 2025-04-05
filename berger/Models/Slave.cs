using berger.Pages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace berger.Models
{
    public class Slave
    {
        private int serverPort = 0;
        private TcpListener server;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Task serverTask;
        public IPAddress serverIP;
        private static TcpClient masterClient;
        public event Action<Tuple<string, int>>? ReceivedMessage;
        private int RowCounter = 0;
        private int badMessageCounter = 0;
        private int goodMessageCounter = 0;   
        private List<string> processedMessages = new List<string>();


        //zmienne do symulowania bledow
        public static string bitFlipMask = "----------------";
        public static int serverDragTime = 0;
        private static ManualResetEvent _pauseEvent = new ManualResetEvent(true); // Początkowo wątek nie jest zatrzymany

        static ConcurrentDictionary<string, TcpClient> connectedClients = new ConcurrentDictionary<string, TcpClient>();
        static ConcurrentDictionary<string, TcpClient> outgoingClients = new ConcurrentDictionary<string, TcpClient>();
        public Slave()
        {
            server = new TcpListener(IPAddress.Any, serverPort);
            server.Start();
            serverIP = ((IPEndPoint)server.LocalEndpoint).Address;
            serverPort = ((IPEndPoint)server.LocalEndpoint).Port;
            serverTask = Task.Run(() => AcceptClients(cancellationTokenSource.Token));
            Console.WriteLine($"Serwer slave nasłuchuje na porcie {serverPort}...");
        }
        public Slave(string masterIP, int masterPort)
        {
            server = new TcpListener(IPAddress.Any, serverPort);
            server.Start();
            serverIP = ((IPEndPoint)server.LocalEndpoint).Address;
            serverPort = ((IPEndPoint)server.LocalEndpoint).Port;
            serverTask = Task.Run(() => AcceptClients(cancellationTokenSource.Token));
            Console.WriteLine($"Serwer slave nasłuchuje na porcie {serverPort}...");

            masterClient = new TcpClient(masterIP, masterPort);

            NetworkStream stream = masterClient.GetStream();

            string message = serverPort.ToString();
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            stream.Flush();

            Task.Run(() => HandleMaster());
        }

        private void HandleMaster()
        {
            NetworkStream stream = masterClient.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Otrzymano: {message} od mastera"); //tu otrzymamy wiadomosc z portem do polaczenia
                  

                    if (int.TryParse(message, out int port))
                    {
                        //Application.Current.Dispatcher.Invoke(() =>
                        //{
                        //    MessageBox.Show($"Otrzymano informacje o podlaczeniu sie na port: {port}");
                        //});
                        ConnectToClient("127.0.0.1", port);
                    }
                    else
                    {
                        Console.WriteLine($"Wiadomość '{message}' nie jest poprawnym numerem portu.");
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd mastera: {ex.Message}");
            }
            finally
            {
                masterClient.Close();
            }
        }
        private void AcceptClients(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (server.Pending())
                    {
                        TcpClient client = server.AcceptTcpClient();
                        string clientId = client.Client.RemoteEndPoint.ToString();
                        connectedClients.TryAdd(clientId, client);

                        //Application.Current.Dispatcher.Invoke(() =>
                        //{
                        //    MessageBox.Show($"Nowy klient: {clientId}");
                        //});
                        Task.Run(() => HandleClient(client, clientId));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd serwera: {ex.Message}");
            }
        }
        private void HandleClient(TcpClient client, string clientId)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    _pauseEvent.WaitOne();
                    Thread.Sleep(serverDragTime);

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    DateTime receivedDate = DateTime.Now;
                    Console.WriteLine($"Otrzymano: {message} od {clientId}"); //tu otrzymamy wiadomosc z kodem bergera prawdopodobnie tylko            

                    //if (message.Length < 21)
                    //{
                    //    Application.Current.Dispatcher.Invoke(() =>
                    //    {
                    //        MessageBox.Show($"Wiadomość za krótka! Otrzymano {message}");
                    //    });
                    //    continue;
                    //}

                    bool isCorrect = VerifyBergerCode(message, out string originalMessage, out string  berger);
                    //zawsze wyswietlamy wiadomosci
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ReceivedMessagesListPage.ReceivedMessageList.Add(new ListViewTemplates.ReceivedMessageRow() { Id = RowCounter, ReceivedMessage = originalMessage, BergerCode = berger, ErrorFlag = !isCorrect, ReceivedDate = receivedDate });
                        RowCounter++;
                        //MessageBox.Show($"Otrzymano {message} ");
                    });
                    //jezeli wiadomosc niepoprawna to wyslanie komunikatu do mastera
                    if (!isCorrect)
                    {
                        badMessageCounter++;
                        //Application.Current.Dispatcher.Invoke(() =>
                        //{
                        //    MessageBox.Show($"Błąd: Otrzymano uszkodzoną wiadomość {message}");
                        //});
                        SendMessageToMaster("ACK: ERROR");
                        continue;
                    }
                    //jezeli poprawna to wyslanie komunikatu do mastera, nalozenie zmiany bitu jezeli jest, rozeslanie dalej tej wiadomosci
                    goodMessageCounter++;
                    SendMessageToMaster("ACK: OK");

                    if (bitFlipMask.Any(c => c == '0' || c == '1'))
                    {
                        StringBuilder sb = new StringBuilder(message);
                        for (int i = 0; i < Math.Min(bitFlipMask.Length, message.Length); i++)
                        {
                            if (bitFlipMask[i] == '0')
                            {
                                sb[i] = '0';
                            }
                            else if (bitFlipMask[i] == '1')
                            {
                                sb[i] = '1';
                            }
                        }
                        message = sb.ToString();
                    }

                    BroadcastMessage(message, clientId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd klienta {clientId}: {ex.Message}");
            }
            finally
            {
                connectedClients.TryRemove(clientId, out _);
                client.Close();
            }
        }
        public void BroadcastMessage(string message, string senderId)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            List<string> disconnectedClients = new List<string>();

            foreach (var kvp in connectedClients)
            {
                if (kvp.Key != senderId)
                {
                    try
                    {
                        kvp.Value.GetStream().Write(messageBytes, 0, messageBytes.Length);
                    }
                    catch (IOException)
                    {
                        Console.WriteLine($"Błąd: Klient {kvp.Key} rozłączył się.");
                        disconnectedClients.Add(kvp.Key);
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine($"Błąd sieciowy podczas wysyłania do klienta {kvp.Key}.");
                        disconnectedClients.Add(kvp.Key);
                    }
                    catch (ObjectDisposedException)
                    {
                        Console.WriteLine($"Błąd: Klient {kvp.Key} został już usunięty.");
                        disconnectedClients.Add(kvp.Key);
                    }
                }
            }

            foreach (var kvp in outgoingClients)
            {
                if (kvp.Key != senderId)
                {
                    try
                    {
                        kvp.Value.GetStream().Write(messageBytes, 0, messageBytes.Length);
                    }
                    catch (IOException)
                    {
                        Console.WriteLine($"Błąd: Klient {kvp.Key} rozłączył się.");
                        disconnectedClients.Add(kvp.Key);
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine($"Błąd sieciowy podczas wysyłania do klienta {kvp.Key}.");
                        disconnectedClients.Add(kvp.Key);
                    }
                    catch (ObjectDisposedException)
                    {
                        Console.WriteLine($"Błąd: Klient {kvp.Key} został już usunięty.");
                        disconnectedClients.Add(kvp.Key);
                    }
                }
            }

            foreach (var clientId in disconnectedClients)
            {
                connectedClients.TryRemove(clientId, out _);
                outgoingClients.TryRemove(clientId, out _);
            }
        }
        private bool VerifyBergerCode(string messageWithBerger, out string originalMessage, out string bergerCode)
        {
            originalMessage = messageWithBerger.Substring(0, messageWithBerger.Length - 5);
            bergerCode = messageWithBerger.Substring(messageWithBerger.Length - 5);

            int actualZeroCount = originalMessage.Count(c => c == '0');
            int expectedZeroCount = Convert.ToInt32(bergerCode, 2);
            bool isValid = actualZeroCount == expectedZeroCount;

            return isValid;
        }
        public void ConnectToClient(string ip, int port)
        {
            try
            {
                TcpClient client = new TcpClient(ip, port);
                string clientId = ip + ":" + port;
                outgoingClients.TryAdd(clientId, client);

                Console.WriteLine($"Połączono z {clientId}");

                Task.Run(() => HandleClient(client, clientId));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd połączenia z {ip}:{port} - {ex.Message}");
            }
        }
        public void StopServer()
        {
            Console.WriteLine("Zamykanie serwera...");
            cancellationTokenSource.Cancel();

            try
            {
                server.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas zatrzymywania serwera: {ex.Message}");
            }

            foreach (var kvp in connectedClients)
            {
                kvp.Value.Close();
            }
            
            foreach (var kvp in outgoingClients)
            {
                kvp.Value.Close();
            }

            connectedClients.Clear();
            outgoingClients.Clear();
            masterClient.Close();
            Console.WriteLine("Serwer został zamknięty.");
        }
        public static void SendMessageToMaster(string statusMessage)
        {
            try
            {
                if (masterClient != null && masterClient.Connected)
                {
                    NetworkStream stream = masterClient.GetStream();
                    byte[] response = Encoding.UTF8.GetBytes(statusMessage);
                    stream.Write(response, 0, response.Length);
                    stream.Flush();
                    Console.WriteLine($"Wysłano do mastera: {statusMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd przy wysyłaniu do mastera: {ex.Message}");
            }
        }
        public int GetServerPort()
        {
            return serverPort;
        }
        public static void PauseServer()
        {
            _pauseEvent.Reset();
        }

        public static void ResumeServer()
        {
            _pauseEvent.Set();
        }

    }
}
