using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace berger.Models
{
    public class Master
    {
        private int serverPort = 8888;
        private TcpListener server;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Task serverTask;
        public event Action<Tuple<string, int>>? ClientConnected;

        public ConcurrentDictionary<string, Tuple<TcpClient, int>> connectedClients = new ConcurrentDictionary<string, Tuple<TcpClient, int>>();

        public Master()
        {
            server = new TcpListener(IPAddress.Any, serverPort);
            server.Start();
            Console.WriteLine("Serwer master nasłuchuje na porcie 8888...");

            serverTask = Task.Run(() => AcceptClients(cancellationTokenSource.Token));
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
                        connectedClients.TryAdd(clientId, new Tuple<TcpClient, int>(client, 0));

                        Console.WriteLine($"Nowy klient: {clientId}");
                        

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
                    Console.WriteLine($"Otrzymano: {message} od {clientId}");

                    if (int.TryParse(message, out int port))
                    {
                        Tuple<string, int> clientData = new Tuple<string, int>(clientId, port);
                        ClientConnected?.Invoke(clientData);
                        connectedClients[clientId] = new Tuple<TcpClient, int>(client, port);
                    }
                    else
                    {
                        Console.WriteLine($"Wiadomość '{message}' nie jest poprawnym numerem portu.");
                    }
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
        public void SendConnectionDetailsMessage(string firstRightClickedElipse, string clickedEllipse)
        {

            string message = connectedClients[clickedEllipse].Item2.ToString();
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            connectedClients[firstRightClickedElipse].Item1.GetStream().Write(messageBytes, 0, messageBytes.Length);
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
                kvp.Value.Item1.Close();
            }

            connectedClients.Clear();
            Console.WriteLine("Serwer został zamknięty.");
        }
        public int GetServerPort()
        {
            return serverPort;
        }
    }

}
