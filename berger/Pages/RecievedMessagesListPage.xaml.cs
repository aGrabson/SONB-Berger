using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using berger.ListViewTemplates;

namespace berger.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy RecievedMessagesListPage.xaml
    /// </summary>
    public partial class RecievedMessagesListPage : Page
    {
        public ObservableCollection<RecivedMessageRow> RecivedMessageList { get; } = new ObservableCollection<RecivedMessageRow>();
        public RecievedMessagesListPage()
        {
            InitializeComponent();
            listView.ItemsSource = RecivedMessageList;
            RecivedMessageList.Add(new RecivedMessageRow() { Id = 1, RecivedMessage = "Test", ErrorFlag = false });
            listView.SizeChanged += (s, e) => ResizeLastColumn();

        }
        private void ResizeLastColumn()
        {
            GridView gridView = listView.View as GridView;
            if (gridView != null && gridView.Columns.Count > 0)
            {
                double totalWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth;
                for (int i = 0; i < gridView.Columns.Count - 1; i++)
                {
                    totalWidth -= gridView.Columns[i].ActualWidth;
                }
                gridView.Columns[gridView.Columns.Count - 1].Width = totalWidth;
            }
        }


    }
}
