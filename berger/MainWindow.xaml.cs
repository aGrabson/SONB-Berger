using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace berger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static ContentControl mainContentRef;
        public MainWindow()
        {
            InitializeComponent();
            mainContentRef = MainContent;
            mainContentRef.Content = new Pages.AppMode();
        }
        public static void NavigateToPage(UserControl userControl)
        {
            if (userControl != null && mainContentRef != null)
                mainContentRef.Content = userControl;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string serverIP = "127.0.0.1";
                int port = 8888;

                TcpClient client = new TcpClient(serverIP, port);
                Console.WriteLine("Nawiązano połączenie z serwerem.");

                NetworkStream stream = client.GetStream();

                while (true)
                {
                    Console.Write("Wprowadź wiadomość: ");
                    string message = Console.ReadLine();

                    byte[] dataToSend = Encoding.ASCII.GetBytes(message);

                    stream.Write(dataToSend, 0, dataToSend.Length);

                    byte[] responseData = new byte[1024];
                    int bytesRead = stream.Read(responseData, 0, responseData.Length);
                    string responseMessage = Encoding.ASCII.GetString(responseData, 0, bytesRead);
                    Console.WriteLine("Odpowiedź od serwera: " + responseMessage);
                }

                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd: " + ex.Message);
            }
        }
    }
}