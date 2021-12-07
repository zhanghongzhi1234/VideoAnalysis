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
using TFRSCli;
using TFRSUtil;
using Core;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Net;
using System.Runtime.InteropServices;

namespace VideoManager
{
    /// <summary>
    /// Interaction logic for Facial.xaml
    /// </summary>
    public partial class FacialWindow : Window
    {
        [DllImport("USER32.DLL", SetLastError = true)]
        public static extern int GetSystemMetrics(int nIndex);
        private uint ScreenWidth = (uint)GetSystemMetrics(0);
        private uint ScreenNumber = (uint)GetSystemMetrics(80);

        ICameraControl cameraControl;
        private DispatcherTimer m_timer1;
        public DisplayWindow displayWindow = null;
        public EnrollWindow enrollWindow = null;
        public ManageWindow manageWindow = null;
        public FacialDetailWindow facialDetailWindow = null;
        public ObservableCollection<User> results = new ObservableCollection<User>();
        public ObservableCollection<User> originalList = new ObservableCollection<User>();
        int totalPersonDetected = 0;
        int snapIndex = 1;

        public FacialWindow()
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
            if (enrollWindow == null)
            {   //just init, not show
                enrollWindow = new EnrollWindow();
                enrollWindow.SetFacialWindow(this);
                enrollWindow.ShowInTaskbar = false;
            }
            if (manageWindow == null)
            {   //just init, not show
                manageWindow = new ManageWindow();
                manageWindow.SetFacialWindow(this);
                manageWindow.ShowInTaskbar = false;
                //manageWindow.Owner = this;
            }
            if (facialDetailWindow == null)
            {   //just init, not show
                facialDetailWindow = new FacialDetailWindow();
                facialDetailWindow.SetFacialWindow(this);
                facialDetailWindow.ShowInTaskbar = false;
                //manageWindow.Owner = this;
            }

            this.Loaded += new RoutedEventHandler(Window_Loaded_1);
            listView1.ItemsSource = results;
        }

        /*public void ReloadTemplates()
        {
            if (CachedMap.Instance.pTfrs.isInitialized == true)
            {
                int iRet = TFRSCli_.Tfrs_ERR_SUCCESS;
                string folder_template_gallery = CachedMap.Instance.folder_template_gallery;
                //CachedMap.Instance.pTfrs.ClearAllTemplates();
                //int num = 0;
                //int temp = CachedMap.Instance.pTfrs.pTfrs.GetNumberOfTemplateLoad(ref num);
                iRet = CachedMap.Instance.pTfrs.LoadTemplates(folder_template_gallery);
                if (TFRSCli_.Tfrs_ERR_SUCCESS != iRet)
                {
                    MessageBox.Show("Cannot Load Templates!", "Display Manager");
                }
            }
            else
            {
                MessageBox.Show("Sdk not initialized, " + (CachedMap.Instance.pTfrs.noLicense == true ? "invalid License!" : "!"), "Display Manager");
            }
            int num1 = 0;
            int temp1 = CachedMap.Instance.pTfrs.pTfrs.GetNumberOfTemplateLoad(ref num1);
        }*/

        //for page jump
        public void SetDisplayWindow(DisplayWindow displayWindow)
        {
            this.displayWindow = displayWindow;
        }

        //for page jump
        public void SetEnrollWindow(EnrollWindow enrollWindow)
        {
            this.enrollWindow = enrollWindow;
        }

        //for page jump
        public void SetManageWindow(ManageWindow manageWindow)
        {
            this.manageWindow = manageWindow;
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

        private void btnStation_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Point point = Mouse.GetPosition(this);
            /*stationSelect.Left = point.X - 5;
            stationSelect.Top = point.Y - 5;
            stationSelect.SetStationWindow(this.stnPage);
            stationSelect.Show();
            stationSelect.Focus();*/
        }

        void stationSelect_MouseLeave(object sender, MouseEventArgs e)
        {
            //stationSelect.Hide();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            //Application.Current.Shutdown();
            //System.Environment.Exit(0);       //why it cause crash in this program?
            displayWindow.Show();
            displayWindow.Focus();
            StopDetection();
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
            DebugUtil.Instance.LOG.Info("Camera type: " + cameraType);
            if (cameraType == "General")
            {
                cameraControl = new GeneralCameraControl();
            }
            cameraControl.Create(panel1);
            cameraControl.Initialize(CachedMap.Instance.cameraRefreshInterval, CachedMap.Instance.snapURL, CachedMap.Instance.videoSnapURL);

            m_timer1 = new DispatcherTimer();
            string strInvertal = CachedMap.Instance.GetRunParam("ProbeInterval");
            m_timer1.Interval = TimeSpan.FromMilliseconds(Convert.ToDouble(strInvertal));
            m_timer1.Tick += new EventHandler(timer1_Tick);

            btnReset_Click(null, null);
        }

        /// <summary>
        /// A method for matching M probes to N galleries in 1toN manner
        /// </summary>
        /// <returns>error code from TFRSCli_ object</returns>
        public int OneToN(Bitmap inbmp)
        {
            DebugUtil.Instance.LOG.Debug("Function Entered: OneToN");
            int iRet = TFRSCli_.Tfrs_ERR_SUCCESS;
            List<string> result_list = new List<string>();
            if (inbmp == null)
            {
                return iRet;
            }
            DebugUtil.Instance.LOG.Debug("OneToN Start");
            iRet = CachedMap.Instance.pTfrs.OneToN(inbmp, CachedMap.Instance.numCandidates, ref result_list);
            DebugUtil.Instance.LOG.Debug("OneToN End");

            if (TFRSCli_.Tfrs_ERR_SUCCESS != iRet)
            {
                return iRet;
            }

            foreach (string s in result_list)
            {
                Console.WriteLine(s);
                string[] result = s.Split(',');
                int ID = Convert.ToInt32(result[0]);
                double score = Convert.ToDouble(result[1]);
                if (score > CachedMap.Instance.score)
                {
                    Template temp = CachedMap.Instance.templateMap[ID];
                    string tempFileName = CachedMap.Instance.folder_image_probe + snapIndex.ToString() + ".jpg";
                    inbmp.Save(tempFileName);
                    //ImageSource tempFileName = Imaging.CreateBitmapSourceFromHBitmap(inbmp, IntPtr.Zero, Int32Rect.Empty, Imaging.BitmapSizeOptions.FromEmptyOptions());
                    snapIndex++;
                    string strPhoto = CachedMap.Instance.folder_image_gallery + temp.pkey.ToString() + ".jpg";
                    //bool exist = results.Any(cus => cus.ID == temp.pkey);
                    User user = results.FirstOrDefault(i => i.ID == temp.pkey);
                    if (user == null)
                    {
                        totalPersonDetected++;
                        txtInfo.Text = "Number Of Person Detected: " + totalPersonDetected.ToString();
                        results.Insert(0, new User() { ID = temp.pkey, Photo = strPhoto, Name = temp.name, Type = temp.type, Time = DateTime.Now.ToString(@"M/d/yyyy h:mm:ss tt"), Note = temp.note, CameraName = "Camera 0002", RealPhoto = tempFileName });
                    }
                    else
                    {
                        int index = results.IndexOf(user);
                        results.Move(index, 0);
                        user.Time = DateTime.Now.ToString(@"M/d/yyyy h:mm:ss tt");
                        user.RealPhoto = tempFileName;
                        CollectionViewSource.GetDefaultView(results).Refresh();
                        /*results = new ObservableCollection<User>(results.OrderByDescending(x => DateTime.ParseExact(x.Time, "M/d/yyyy h:mm:ss tt",
                                       System.Globalization.CultureInfo.InvariantCulture)));*/
                    }
                    //save original list
                    originalList.Add(new User() { ID = temp.pkey, Photo = strPhoto, Name = temp.name, Type = temp.type, Time = DateTime.Now.ToString(@"M/d/yyyy h:mm:ss tt"), Note = temp.note, CameraName = "Camera 0002", RealPhoto = tempFileName });
                }
            }
            //ICollectionView view = CollectionViewSource.GetDefaultView(results);
            //view.Refresh();
            DebugUtil.Instance.LOG.Debug("Function Exited: OnetoN");
            return iRet;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //string filePath = CachedMap.Instance.folder_image_probe + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff") + ".jpg";
            DebugUtil.Instance.LOG.Debug("Capture Start");
            Bitmap inBmp = cameraControl.Capture();
            DebugUtil.Instance.LOG.Debug("Capture End");
            OneToN(inBmp);
            DebugUtil.Instance.LOG.Debug("Capture Image Dispose");
            inBmp.Dispose();
        }

        //not use now
        /*private int GetNumberOfPeopleDetected()
        {
            int nRet = (from x in results
                  select x.ID).Distinct().Count();

            return nRet;
        }*/

        private void btnHomepage_Click(object sender, RoutedEventArgs e)
        {
            displayWindow.Show();
            displayWindow.Focus();
            StopDetection();
        }

        private void btnEnroll_Click(object sender, RoutedEventArgs e)
        {
            if (enrollWindow == null)
            {
                enrollWindow = new EnrollWindow();
                enrollWindow.SetFacialWindow(this);
            }
            enrollWindow.Show();
            enrollWindow.Focus();
            StopDetection();
        }

        private void btnManage_Click(object sender, RoutedEventArgs e)
        {
            if (manageWindow == null)
            {
                manageWindow = new ManageWindow();
                manageWindow.SetFacialWindow(this);
            }
            manageWindow.Show();
            manageWindow.Focus();
            StopDetection();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (btnStart.Content.ToString() == "STOP")
            {
                StopDetection();
            }
            else
            {
                StartDetection();
            }
        }

        private void StartDetection()
        {
            DebugUtil.Instance.LOG.Info("Start Detection");
            cameraControl.Start();
            if (CachedMap.Instance.pTfrs.isInitialized == true)
            {
                m_timer1.Start();
            }
            else
            {
                MessageBox.Show("Sdk not initialized, " + (CachedMap.Instance.pTfrs.noLicense == true ? "invalid License!" : "!"), "Display Manager");
            }
            btnStart.Content = "STOP";
        }

        private void StopDetection()
        {
            DebugUtil.Instance.LOG.Info("Stop Detection");
            m_timer1.Stop();
            cameraControl.Stop();
            btnStart.Content = "START DETECTION";
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            results.Clear();
            originalList.Clear();
            totalPersonDetected = 0;
            txtInfo.Text = "Number Of Person Detected: 0";

            System.IO.DirectoryInfo di = new DirectoryInfo(CachedMap.Instance.folder_image_probe);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }

        /// <summary>
        /// A method to enroll templates by converting facial image files readed a specified folder
        /// </summary>
        /// <returns> error code from TFRSCli_ object</returns>
        public int EnrollOne(string inimage, string outtemplate)
        {
            int iRet = TFRSCli_.Tfrs_ERR_SUCCESS;

            iRet = CachedMap.Instance.pTfrs.EnrollOne(inimage, outtemplate);

            return iRet;
        }

        private void Window_Deactivated_1(object sender, EventArgs e)
        {
            /*m_timer1.Stop();
            cameraControl.Stop();
            btnStart.Content = "START DETECTION";*/
        }

        private void Window_Closing_1(object sender, CancelEventArgs e)
        {
            try
            {
                cameraControl.CleanUp();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void listViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            StopDetection();
            ListViewItem item = sender as ListViewItem;
            object obj = item.Content;
            User user = (User)obj;
            facialDetailWindow.Show();
            facialDetailWindow.Focus();
            facialDetailWindow.SetUserID(user.ID);
            this.Hide();
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
    }
}
