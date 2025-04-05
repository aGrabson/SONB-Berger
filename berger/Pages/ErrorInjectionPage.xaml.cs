using berger.Models;
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
        private bool isBitFlip = false;
        private bool isPaused = false;
        private bool isOverload = false;
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
            bool tmpBool = isBitFlip;
            string bitValue = "";
            foreach (TextBox textBox in textBoxes)
            {
                bitValue += textBox.Text=="0"||textBox.Text=="1"?textBox.Text:"-";
            }
            Slave.bitFlipMask = bitValue;
            isBitFlip = bitValue.Any(c => c == '0' || c == '1');
            MessageBox.Show($"Maska: {bitValue}", "Sukces");

            if (tmpBool != isBitFlip)
            {
                Slave.SendMessageToMaster($"INJ: bitflip");
            }

        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            bool tmpBool = isPaused;
            if (isPaused)
            {
                isPaused = false;
                disconnectButton.Content = "Odłącz";
                Slave.ResumeServer();
            }
            else
            {
                isPaused = true;
                disconnectButton.Content = "Połącz";
                Slave.PauseServer();
            }
            if (tmpBool != isPaused)
            {
                Slave.SendMessageToMaster($"INJ: disconnect");
            }
        }

        private void OverloadButton_Click(object sender, RoutedEventArgs e)
        {
            bool tmpBool = isOverload;
            isOverload = int.Parse(DelayTextBox.Text) != 0;
            Slave.serverDragTime = int.Parse(DelayTextBox.Text);
            ActualDelay.Text = DelayTextBox.Text;

            if (tmpBool != isOverload) 
            { 
                Slave.SendMessageToMaster($"INJ: overload");
            }

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
