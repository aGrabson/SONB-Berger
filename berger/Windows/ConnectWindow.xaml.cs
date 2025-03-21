using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace berger.Windows
{
    /// <summary>
    /// Logika interakcji dla klasy ConnectWindow.xaml
    /// </summary>
    public partial class ConnectWindow : Window
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public ConnectWindow()
        {
            InitializeComponent();
        }

        private void Connect_Button_Click(object sender, RoutedEventArgs e)
        {
            IpAddress = ip.Text;
            Port = int.Parse(port.Text);
            this.Close();
        }
    }
}
