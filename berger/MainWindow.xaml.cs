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
    }
}