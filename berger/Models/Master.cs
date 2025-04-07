using berger.Pages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace berger.Models
{
    public class Master
    {
        private int serverPort = 8888;
        private TcpListener server;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Task serverTask;
        public ConcurrentDictionary<string, Tuple<TcpClient, int>> connectedClients = new ConcurrentDictionary<string, Tuple<TcpClient, int>>();

        //dla UI
        public event Action<Tuple<string, int>>? ClientConnected;
        public event Action<string, Brush> AcknowledgmentReceived;
        public event Action<string, string> ErrorInjection;

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

                        //Console.WriteLine($"Nowy klient: {clientId}");
                        

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

                    if (message.StartsWith("ACK:"))
                    {
                        string status = message.Substring(4).Trim();
                        Console.WriteLine($"Potwierdzenie od {clientId}: {status}");

                        if (status == "OK")
                        {
                            AcknowledgmentReceived?.Invoke(clientId, Brushes.LightGreen);
                        }
                        else if (status == "ERROR")
                        {
                            AcknowledgmentReceived?.Invoke(clientId, Brushes.Red);
                        }
                    }
                    else if (message.StartsWith("INJ:"))
                    {
                        string status = message.Substring(4).Trim();
                        Console.WriteLine($"Włączenie wstrzykiwania błędu od {clientId}: {status}");

                        ErrorInjection?.Invoke(clientId, status);
                    }
                    else if (message.StartsWith("INF:"))
                    {
                        string info = message.Substring(4).Trim();
                        string[] parts = info.Split(',');

                        int number1 = int.Parse(parts[0]);
                        int number2 = int.Parse(parts[1]);
                        Console.WriteLine($"Od klienta dostano informacje, poprawne wiadomości: {number2}, ogólna liczba wiadomości {number1}");

                        var existingInfo = GraphEditor.MessagesInfoList
                                           .FirstOrDefault(row => row.ClientID == clientId);

                        if (existingInfo != null)
                        {
                            existingInfo.CorrectNumberMessages = number2;
                            existingInfo.NumberMessages = number1;
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                GraphEditor.MessagesInfoList.Add(new ListViewTemplates.MessageInfoRow
                                {
                                    ClientID = clientId,
                                    ClientPort = connectedClients[clientId].Item2.ToString(),
                                    CorrectNumberMessages = number2,
                                    NumberMessages = number1
                                });
                            });
                            
                        }
                    }
                    else if(int.TryParse(message, out int port))
                    {
                        Tuple<string, int> clientData = new Tuple<string, int>(clientId, port);
                        ClientConnected?.Invoke(clientData);
                        connectedClients[clientId] = new Tuple<TcpClient, int>(client, port);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            GraphEditor.MessagesInfoList.Add(new ListViewTemplates.MessageInfoRow
                            {
                                ClientID = clientId,
                                ClientPort = connectedClients[clientId].Item2.ToString(),
                                CorrectNumberMessages = 0,
                                NumberMessages = 0
                            });
                        });
                    }
                    else
                    {
                        //Console.WriteLine($"Wiadomość '{message}' nie jest poprawnym numerem portu.");
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
