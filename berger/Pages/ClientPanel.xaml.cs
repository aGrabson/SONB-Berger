using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using berger.Windows;
namespace berger.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy ClientPanel.xaml
    /// </summary>
    public partial class ClientPanel : UserControl
    {
        public ClientPanel()
        {
            InitializeComponent();
            ConnectWindow window = new ConnectWindow();
            window.ShowDialog();

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
    }
}
