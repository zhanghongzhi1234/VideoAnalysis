using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;
using Core;

namespace VideoManager
{
    /// <summary>
    /// Interaction logic for Object.xaml
    /// </summary>
    public partial class CrowdWindow : Window
    {
        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        private const int SW_HIDE = 0;              //{隐藏}
        private const int WS_SHOWNORMAL = 1;        //{用最近的大小和位置显示, 激活}
        private const int SW_NORMAL = 1;            //{同 SW_SHOWNORMAL}
        private const int SW_SHOWMINIMIZED = 2;     //{最小化, 激活}
        private const int SW_SHOWMAXIMIZED = 3;     //{最大化, 激活}
        private const int SW_MAXIMIZE = 3;          //{同 SW_SHOWMAXIMIZED}
        private const int SW_SHOWNOACTIVATE = 4;    //{用最近的大小和位置显示, 不激活}
        private const int SW_SHOW = 5;              //{同 SW_SHOWNORMAL}
        private const int SW_MINIMIZE = 6;          //{最小化, 不激活}
        private const int SW_SHOWMINNOACTIVE = 7;   //{同 SW_MINIMIZE}
        private const int SW_SHOWNA = 8;            //{同 SW_SHOWNOACTIVATE}
        private const int SW_RESTORE = 9;           //{同 SW_SHOWNORMAL}
        private const int SW_SHOWDEFAULT = 10;      //{同 SW_SHOWNORMAL}
        private const int SW_MAX = 10;              //{同 SW_SHOWNORMAL}

        [DllImport("USER32.DLL", SetLastError = true)]
        public static extern int GetSystemMetrics(int nIndex);
        private uint ScreenWidth = (uint)GetSystemMetrics(0);
        private uint ScreenNumber = (uint)GetSystemMetrics(80);

        public DisplayWindow displayWindow = null;
        
        public CrowdWindow()
        {
            InitializeComponent();
        }

        public void SetDisplayWindow(DisplayWindow displayWindow)
        {
            this.displayWindow = displayWindow;
        }

        private void InitSize()
        {
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.AllScreens[CachedMap.Instance.placeScreenNumber - 1];
            double x1 = screen.Bounds.Width;
            double y1 = screen.Bounds.Height;
            this.Width = x1;
            this.Height = y1;
            vbContent.Width = x1;
            vbContent.Height = y1;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            displayWindow.Show();
            displayWindow.Focus();
            //StopDetection();
        }

        private void menuButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Button btn = (Button)sender;
            btn.Foreground = System.Windows.Media.Brushes.Black;
        }

        private void menuButton_MouseLeave(object sender, MouseEventArgs e)
        {
            Button btn = (Button)sender;
            btn.Foreground = System.Windows.Media.Brushes.White;
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            try
            {
                InitSize();
                SetScreenPosition();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                System.Environment.Exit(0);
            }
            Navigate(CachedMap.Instance.crowdURL);
        }

        private void Navigate(string url)
        {
            // Get URI to navigate to
            Uri uri = new Uri(url, UriKind.RelativeOrAbsolute);

            // Only absolute URIs can be navigated to
            if (!uri.IsAbsoluteUri)
            {
                MessageBox.Show("The Address URI must be absolute eg 'http://www.microsoft.com'");
                return;
            }

            // Navigate to the desired URL by calling the .Navigate method
            this.web1.Navigate(uri);
        }

        private void SetScreenPosition()
        {
            int PlaceScreenNumber = CachedMap.Instance.placeScreenNumber;
            if (PlaceScreenNumber <= ScreenNumber && ScreenNumber > 1)
            {
                if (WindowState == WindowState.Normal)
                {
                    Left += (PlaceScreenNumber - 1) * ScreenWidth;
                }
                else if (WindowState == WindowState.Maximized)
                {
                    Hide();
                    WindowState = WindowState.Normal;
                    //Left = (PlaceScreenNumber - 1) * ScreenWidth - 7;
                    Left = CachedMap.Instance.screenX;
                    Top = CachedMap.Instance.screenY;
                    DebugUtil.Instance.LOG.Info("Set Screen Position: Left=" + Left + ", Top=" + Top);
                    Application.Current.MainWindow.WindowState = WindowState.Maximized;
                    Show();
                }
            }
        }

        private void btnHomepage_Click(object sender, RoutedEventArgs e)
        {
            //panel2.Background = System.Windows.Media.Brushes.Black;
        }

        private void Window_Closing_1(object sender, CancelEventArgs e)
        {
            try
            {
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

}
