using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace berger.ListViewTemplates
{
    public class MessageInfoRow : INotifyPropertyChanged
    {
        public string ClientID { get; set; }
        public string ClientPort { get; set; }

        private int correctNumberMessages;
        public int CorrectNumberMessages
        {
            get => correctNumberMessages;
            set
            {
                if (correctNumberMessages != value)
                {
                    correctNumberMessages = value;
                    OnPropertyChanged(nameof(CorrectNumberMessages));
                }
            }
        }

        private int numberMessages;
        public int NumberMessages
        {
            get => numberMessages;
            set
            {
                if (numberMessages != value)
                {
                    numberMessages = value;
                    OnPropertyChanged(nameof(NumberMessages));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
