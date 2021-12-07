using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using VideoManager;

namespace MyConverters
{
    [ValueConversion(typeof(string), typeof(System.Windows.Media.Color))]
    public class MyBkColorConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ListViewItem item = (ListViewItem)value;
            ListView listView =
            ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            // Get the index of a ListViewItem
            int index =
                listView.ItemContainerGenerator.IndexFromContainer(item);
            ObservableCollection<User> results = (ObservableCollection<User>)listView.ItemsSource;
            User user = results[index];

            string type = user.Type;
            System.Windows.Media.Brush brush = System.Windows.Media.Brushes.BlueViolet;
            if(type == "VIP")
                brush = System.Windows.Media.Brushes.Green;
            else if(type == "BLACKLIST")
                brush = System.Windows.Media.Brushes.Red;

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //throw new NotImplementedException();
            return null;
        }
    }
}
