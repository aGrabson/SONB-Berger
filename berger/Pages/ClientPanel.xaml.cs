using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using berger.Threads;
using berger.Windows;
namespace berger.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy ClientPanel.xaml
    /// </summary>
    public partial class ClientPanel : UserControl
    {
        public static ListenerThread listenerThread = new ListenerThread(false);
        public ClientPanel()
        {
            InitializeComponent();
            ConnectWindow window = new ConnectWindow();
            window.ShowDialog();
            listenerThread.start();

            Application.Current.Exit += OnApplicationExit;
            Task.Run(() =>
            {
                while (listenerThread.listenerPort == 0)
                {
                    Task.Delay(100).Wait();
                }
                TcpClient client = new TcpClient(window.IpAddress, window.Port);

                //NetworkStream stream = client.GetStream();

                //string message = listenerThread.listenerPort.ToString();
                //byte[] data = Encoding.UTF8.GetBytes(message);
                //stream.Write(data, 0, data.Length);
                //stream.Flush();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Serwer działa na {listenerThread.listenerIP}:{listenerThread.listenerPort}");
                });
            });


        }
        private void ConnectToServer(object sender, RoutedEventArgs e)
        {
            //Dispatcher.Invoke(() =>
            //{
            //    MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            //    mainWindow.clientIp = ipAddress.Text;
            //    mainWindow.clientPort = int.Parse(port.Text);
            //});
            //tabControl.SelectedIndex = 4;
        }
        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            listenerThread.stop();
        }
    }
}
