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

namespace VideoManager
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        public Window2()
        {
            InitializeComponent();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            // Get URI to navigate to
            //Uri uri = new Uri("http://www.google.com", UriKind.RelativeOrAbsolute);
            Uri uri = new Uri("http://192.168.253.149/jpg/image.jpg", UriKind.RelativeOrAbsolute);

            // Only absolute URIs can be navigated to
            if (!uri.IsAbsoluteUri)
            {
                MessageBox.Show("The Address URI must be absolute eg 'http://www.microsoft.com'");
                return;
            }

            // Navigate to the desired URL by calling the .Navigate method
            this.web1.Navigate(uri);
        }
    }
}
