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
using System.Runtime.InteropServices;
using Core;

namespace VideoManager
{
    /// <summary>
    /// Interaction logic for FacialDetailWindow.xaml
    /// </summary>
    public partial class FacialDetailWindow : Window
    {
        [DllImport("USER32.DLL", SetLastError = true)]
        public static extern int GetSystemMetrics(int nIndex);
        private uint ScreenWidth = (uint)GetSystemMetrics(0);
        private uint ScreenNumber = (uint)GetSystemMetrics(80);

        ObservableCollection<User> detailList = new ObservableCollection<User>();
        private System.Windows.Forms.PictureBox m_image = new System.Windows.Forms.PictureBox();
        FacialWindow facialWindow;
        int userID;

        public FacialDetailWindow()
        {
            InitializeComponent();
            m_image.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;

            listView1.ItemsSource = detailList;
            this.Loaded += new RoutedEventHandler(Window_Loaded_1);
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

        public void SetFacialWindow(FacialWindow facialWindow)
        {
            this.facialWindow = facialWindow;
        }

        public void SetUserID(int userID)
        {
            this.userID = userID;
            GetAllByUserID(userID);
        }

        private void btnHomePage_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            facialWindow.Show();
            facialWindow.Focus();
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

            //System.Windows.Forms.Integration.WindowsFormsHost hostPlayer = new System.Windows.Forms.Integration.WindowsFormsHost();
            System.Windows.Forms.Integration.WindowsFormsHost hostImage = new System.Windows.Forms.Integration.WindowsFormsHost();
            hostImage.Child = m_image;
            Image.Children.Add(hostImage);

            //GetAllByUserID(userID);
        }

        private void GetAllByUserID(int userID)
        {
            detailList.Clear();
            foreach (User user in facialWindow.originalList)
            {
                if (user.ID == userID)
                    detailList.Insert(0, user);
            }
        }

        /*private void CleanUp()
        {
            alarms.Clear();
        }*/

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            //alarms.Clear();
            /*alarms.Insert(0, new User()
            {
                Time = DateTime.Now.ToString("yyyy-MM-dd, HH:mm:ss"),
                AlarmID = "1000",
                Title = "test",
                Description = "test",
                State = "haha"
            });*/
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            btnExit_Click(null, null);
        }

        /*private void showAttachmentShot(IImage image)
        {
            using (MemoryStream _stream = new MemoryStream(image.Content))
            {
                m_image.Image = System.Drawing.Image.FromStream(_stream);
            }

        }*/

        private void onSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            User a = listView1.SelectedItem as User;
            if (a == null)
                return;
            //m_image.Load(a.RealPhoto);
            ShowPortrait(a.RealPhoto);
            //facialWindow.GetAlarmByResourceID(a.ID);
            //if (a == null)
              //  return;
            /*
            foreach (IAttachment _attachment in _a.Attachments)
            {
                if (_attachment is IImageAttachment)
                {
                    IImageAttachment imageAttach = _attachment as IImageAttachment;
                    showAttachmentShot(imageAttach.Image);
                }
            }*/

            txtInfo.Text = a.Time;
        }

        private void ShowPortrait(string strPhoto)
        {
            /*BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.UriSource = new Uri(strPhoto);
            image.EndInit();

            imgPortrait.Source = image;*/

            System.Drawing.Image img = new Bitmap(strPhoto);  // create the bitmap
            string imgName = strPhoto;
            m_image.Image = img.GetThumbnailImage(640, 480, null, new IntPtr());
            img.Dispose();  // dispose the bitmap object
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
