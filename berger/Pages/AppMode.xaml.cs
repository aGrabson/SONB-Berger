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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace berger.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy AppMode.xaml
    /// </summary>
    public partial class AppMode : UserControl
    {
        public AppMode()
        {
            InitializeComponent();
        }

        private void ClientMode_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigateToPage(new ClientPanel());
        }

        private void ServerMode_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigateToPage(new GraphEditor());
        }
    }
}
