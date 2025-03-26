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
        private TcpClient masterClient;

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
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Otrzymano informacje o podlaczeniu sie na port: {port}");
                        });
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

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Nowy klient: {clientId}");
                        });
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

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Otrzymano: {message} od {clientId}"); //tu otrzymamy wiadomosc z kodem bergera prawdopobnie tylko

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
        private void BroadcastMessage(string message, string senderId)
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

            foreach (var clientId in disconnectedClients)
            {
                connectedClients.TryRemove(clientId, out _);
                outgoingClients.TryRemove(clientId, out _);
            }
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
    }
}
