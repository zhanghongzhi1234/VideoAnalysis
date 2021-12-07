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
    public class AlarmFrequency
    {
        public enum ESeverity { MINOR, MAJOR, CRITICAL };
        public string Equipment { get; set; }
        public ESeverity Severity { get; set; }
        public string Description { get; set; }
        public string Frequency { get; set; }
        
        
        
        public AlarmFrequency(string eqpt, ESeverity severity, string description, string frequency)
        {
            Equipment = eqpt;
            Severity = severity;
            Description = description;
            Frequency = frequency;
        }
    }
    /// <summary>
    /// Interaction logic for AlarmFrequencyList.xaml
    /// </summary>
    public partial class AlarmFrequencyList : UserControl
    {
        public AlarmFrequencyList()
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
