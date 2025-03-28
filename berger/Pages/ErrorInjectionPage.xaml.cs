using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Logika interakcji dla klasy ErrorInjectionPage.xaml
    /// </summary>
    public partial class ErrorInjectionPage : Page
    {
        private List<TextBox> textBoxes;
        public ErrorInjectionPage()
        {
            InitializeComponent();
            textBoxes = new List<TextBox>
            {
                Bit0, Bit1, Bit2, Bit3,
                Bit4, Bit5, Bit6, Bit7,
                Bit8, Bit9, Bit10, Bit11,
                Bit12, Bit13, Bit14, Bit15
            };
        }

        private void BitFlipButton_Click(object sender, RoutedEventArgs e)
        {
            string bitValue = "";
            foreach (TextBox textBox in textBoxes)
            {
                bitValue += textBox.Text=="0"||textBox.Text=="1"?textBox.Text:"-";
            }
            MessageBox.Show($"Maska: {bitValue}", "Sukces");

        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OverloadButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void BitInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[01]$");
        }
        private void DeleyInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0123456789]$");
        }
    }
}
