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

namespace List
{
    public class Alarm
    {
        public enum ESeverity { MINOR, MAJOR, CRITICAL };
        public DateTime TimeStamp { get; set; }
        public string TimeString { get; set; }
        public string Description { get; set; }
        public ESeverity Severity { get; set; }
        public string Equipment { get; set; }
        public Alarm(DateTime timeStamp, string description, ESeverity severity, string eqpt)
        {
            TimeStamp = timeStamp;
            TimeString = TimeStamp.ToString("dd MMM yyyy HH:mm:ss");
            Description = description;
            Severity = severity;
            Equipment = eqpt;
        }
    }
    /// <summary>
    /// Interaction logic for AlarmList.xaml
    /// </summary>
    public partial class AlarmList : UserControl
    {
        public AlarmList()
        {
            InitializeComponent();
            listView.SelectionMode = SelectionMode.Single;
            ScrollViewer.SetVerticalScrollBarVisibility(this, ScrollBarVisibility.Visible);
            ScrollViewer.SetHorizontalScrollBarVisibility(this, ScrollBarVisibility.Hidden);
        }

        private void ListViewItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item == null)
                return;

            var dataItem = item.DataContext as Alarm;
            if (dataItem == null)
                return;

            //MessageBox.Show(dataItem.Description);
        }
    }
}
