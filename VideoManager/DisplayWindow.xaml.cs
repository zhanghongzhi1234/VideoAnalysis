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
    public partial class DisplayWindow : Window
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

        IQuadControl quadControl;
        public FacialWindow facialWindow = null;
        public CrowdWindow crowdWindow = null;

        public DisplayWindow()
        {
            InitializeComponent();

            /*try
            {
                InitSize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                System.Environment.Exit(0);
            }*/

            if (facialWindow == null)
            {   //just init, not show
                facialWindow = new FacialWindow();
                facialWindow.SetDisplayWindow(this);
                facialWindow.ShowInTaskbar = false;
            }

            if (crowdWindow == null)
            {   //just init, not show
                crowdWindow = new CrowdWindow();
                crowdWindow.SetDisplayWindow(this);
                crowdWindow.ShowInTaskbar = false;
            }
        }

        private void InitSize()
        {
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.AllScreens[CachedMap.Instance.placeScreenNumber - 1];
            //double x1 = SystemParameters.PrimaryScreenWidth;//得到屏幕整体宽度
            //double y1 = SystemParameters.PrimaryScreenHeight;//得到屏幕整体高度
            double x1 = screen.Bounds.Width;
            double y1 = screen.Bounds.Height;
            this.Width = x1;
            this.Height = y1;
            vbContent.Width = x1;
            vbContent.Height = y1;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            //System.Environment.Exit(0);       //why it cause crash in this program?
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

            string cameraType = CachedMap.Instance.cameraType;
            if (cameraType == "General")
            {
                quadControl = new GeneralQuadControl();
            }

            quadControl.Create(panel1, panel2, panel3, panel4, true);
            quadControl.Initialize();

            if (CachedMap.Instance.showCrowd == false)
                btnCrowd.Visibility = Visibility.Hidden;
            //panel1.Background = System.Windows.Media.Brushes.Black;
        }

        /*private void SetScreenPosition()
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
                    Left = (PlaceScreenNumber - 1) * ScreenWidth - 7;
                    Application.Current.MainWindow.WindowState = WindowState.Maximized;
                    Show();
                }
            }
        }*/

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

        private void btnFacial_Click(object sender, RoutedEventArgs e)
        {
            if (facialWindow == null)
            {
                facialWindow = new FacialWindow();
                facialWindow.SetDisplayWindow(this);
            }
            facialWindow.Show();
            facialWindow.Focus();
            StopAllDispaly();
        }

        public void StopAllDispaly()
        {
            quadControl.Start(false, false, false, false);
        }

        private void btnCrowd_Click(object sender, RoutedEventArgs e)
        {
            /*if (crowdWindow == null)
            {
                crowdWindow = new CrowdWindow();
                crowdWindow.SetDisplayWindow(this);
            }
            crowdWindow.Show();
            crowdWindow.Focus();*/
            string url = CachedMap.Instance.crowdURL;
            //launchFromChrome(url);
            if(isAlive(url) == true)
            {
                launchFromChrome(url);
            }
            else
            {
                MessageBox.Show("Cannot Connect to Crowd Counting Server!", "Display Manager");
            }
        }

        private bool isAlive(string url)
        {
            bool ret = true;
            Uri baseUri = new Uri(url);
            string host = baseUri.Host;

            var ping = new System.Net.NetworkInformation.Ping();
            var result = ping.Send(host);
            if (result.Status != System.Net.NetworkInformation.IPStatus.Success)
                ret = false;

            return ret;
        }

        private void launchFromChrome(string url)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
                //process.StartInfo.Arguments = url + " --kiosk --incognito";
                process.StartInfo.Arguments = url + " --incognito";

                process.Start();
            }
        }

        private void btnObject_Click(object sender, RoutedEventArgs e)
        {
            Process instance = RunningInstance("ObjectDetect");
            if (instance == null)
            {
                // Prepare the process to run
                ProcessStartInfo start = new ProcessStartInfo();
                // Enter in the command line arguments, everything you would enter after the executable name itself
                start.Arguments = "screen=" + CachedMap.Instance.placeScreenNumber;
                // Enter the executable to run, including the complete path
                start.FileName = Environment.CurrentDirectory + "/ObjectDetect.exe";
                // Do you want to show a console window?
                start.WindowStyle = ProcessWindowStyle.Hidden;
                start.CreateNoWindow = true;
                int exitCode;

                // Run the external process & wait for it to finish
                using (Process proc = Process.Start(start))
                {
                    proc.WaitForExit();
                    // Retrieve the app's exit code
                    exitCode = proc.ExitCode;
                }
            }
            else
            {   //处理发现的例程
                HandleRunningInstance(instance);
            }
        }

        private Process RunningInstance(string name)
        {
            //Process current = Process.GetCurrentProcess();
            Process ret = null;
            Process[] processes = Process.GetProcessesByName(name);
            if (processes.Count() >= 1)
                ret = processes[0];
            for (int i = 1; i < processes.Count(); i++)
            {
                processes[i].Kill();
            }
            //遍历正在有相同名字运行的例程   
            /*foreach (Process process in processes)
            {
                //忽略现有的例程     
                if (process.Id != current.Id)
                {
                    //确保例程从EXE文件运行       
                    if (Assembly.GetExecutingAssembly().Location.Replace("/", "//") == current.MainModule.FileName)
                    {
                        //返回另一个例程实例         
                        return process;
                    }
                }
            }*/
            //没有其它的例程，返回Null   
            return ret;
        }

        private void HandleRunningInstance(Process instance)
        {
            //确保窗口没有被最小化或最大化   
            ShowWindowAsync(instance.MainWindowHandle, SW_SHOWMAXIMIZED);
            //设置真实例程为foreground window   
            SetForegroundWindow(instance.MainWindowHandle);
        }

        private void Window_Closing_1(object sender, CancelEventArgs e)
        {
            try
            {
                quadControl.CleanUp();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            panel1.Background = System.Windows.Media.Brushes.Black;
            object data = new DataObject(DataFormats.Text, ((TreeViewItem)e.Source).Header.ToString());
            DragDrop.DoDragDrop((DependencyObject)e.Source, data, DragDropEffects.Copy);
        }

        private void item_Drop(object sender, DragEventArgs e)
        {
            DockPanel panel = sender as DockPanel;
            if (panel == null)
                return;
            string cameraName = (string)e.Data.GetData(DataFormats.Text);
            //MessageBox.Show(cameraName);
            //panel1.Background = System.Windows.Media.Brushes.Black;
            switch (panel.Name)
            {
                case "panel1":
                    quadControl.Start(1);
                    break;
                case "panel2":
                    quadControl.Start(2);
                    break;
                case "panel3":
                    quadControl.Start(3);
                    break;
                case "panel4":
                    quadControl.Start(4);
                    break;
                default:
                    break;
            }
        }

        private void item_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            {
                Cursor cursor = CursorHelper.CreateCursor(e.Source as UIElement);
                e.UseDefaultCursors = false;
                Mouse.SetCursor(cursor);
            }

            e.Handled = true;
        }

    }

}
