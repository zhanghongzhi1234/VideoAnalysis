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
using TFRSCli;
using TFRSUtil;

namespace VideoManager
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            string configPath = "c:\\TOSHIBA\\SDK_FRVT2013_Final\\SDK\\dic\\facedata";
            //Console.WriteLine("configPath," + configPath);

            TFRSUtil_ pTfrs = new TFRSUtil_(configPath);
        }
    }
}
