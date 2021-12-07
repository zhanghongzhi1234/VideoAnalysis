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
using Verint.VideoSolutions.Business.Common;
using Verint.VideoSolutions.Business.Client;
using System.Runtime.InteropServices;

namespace ObjectDetect
{
    /// <summary>
    /// Interaction logic for Object.xaml
    /// </summary>
    public partial class ObjectWindow : Window
    {
        [DllImport("USER32.DLL", SetLastError = true)]
        public static extern int GetSystemMetrics(int nIndex);
        private uint ScreenWidth = (uint)GetSystemMetrics(0);
        private uint ScreenNumber = (uint)GetSystemMetrics(80);
        public int placeScreenNumber = 1;

        private NextivaSite m_site;
        private DateTime m_lastDateTime = DateTime.MinValue.ToUniversalTime();
        ObservableCollection<Alarm> alarms = new ObservableCollection<Alarm>();
        //private NextivaVideoControl m_videoCtrl = new NextivaVideoControl();
        private System.Windows.Forms.PictureBox m_image = new System.Windows.Forms.PictureBox();
        //List<Alarm> alarms = new List<Alarm>();

        public ObjectWindow()
        {
            InitializeComponent();

            ParseCommandLine();
            m_image.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            listView1.ItemsSource = alarms;
            this.Loaded += new RoutedEventHandler(Window_Loaded_1);
        }

        private void InitSize()
        {
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.AllScreens[placeScreenNumber - 1];
            double x1 = screen.Bounds.Width;
            double y1 = screen.Bounds.Height;
            this.Width = x1;
            this.Height = y1;
            vbContent.Width = x1;
            vbContent.Height = y1;
        }

        private void btnHomePage_Click(object sender, RoutedEventArgs e)
        {
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

            //System.Windows.Forms.Integration.WindowsFormsHost hostPlayer = new System.Windows.Forms.Integration.WindowsFormsHost();
            System.Windows.Forms.Integration.WindowsFormsHost hostImage = new System.Windows.Forms.Integration.WindowsFormsHost();
            hostImage.Child = m_image;
            Image.Children.Add(hostImage);

            InitVMS();
            m_site.GetManager<IAlarmManager>().AlarmAdded += new AlarmAddedEventHandler(AlarmRetrievalAndNotificationsSampleControl_AlarmAdded);

            //btnGetAll_Click(null, System.Windows.RoutedEventArgs.Empty);
            btnGetAll_Click(null, null);
        }

        private bool InitVMS()
        {
            bool ret = false;
            m_site = new NextivaSite();
            /*string server = "192.168.253.200";
            string port = "5005";
            string userName = "Administrator";
            string password = "cctvware";*/
            
            string server = CachedMap.Instance.GetRunParam("VMSServerIP");
            string port = CachedMap.Instance.GetRunParam("VMSServerPort");
            string userName = CachedMap.Instance.GetRunParam("VMSServerUsername");
            string password = CachedMap.Instance.GetRunParam("VMSServerPassword");

            try
            {
                UriBuilder builder = new UriBuilder("tcp", server, Int32.Parse(port));
                m_site.Initialize(builder.ToString());
                //m_loginMode = m_site.GetAuthenticationMode();
                ret = true;
            }
            catch
            {
                //m_loginMode = LoginModes.Unknown;
                ret = false;
            }

            if (ret == true)
            {
                try
                {
                    m_site.LoginEx(userName, password);
                }
                catch (Exception)
                {
                }
            }
            return ret;
        }

        private void CleanUp()
        {
            alarms.Clear();
            m_site.GetManager<IAlarmManager>().AlarmAdded -= new AlarmAddedEventHandler(AlarmRetrievalAndNotificationsSampleControl_AlarmAdded);
        }

        private void btnViewCamera_Click(object sender, RoutedEventArgs e)
        {
            CameraView cameraWindow = new CameraView(m_site);
            cameraWindow.Show();
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            //FilterWindow filterWindow = new FilterWindow(m_site);
            //filterWindow.Show();
        }

        private void btnManage_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnVideo_Click(object sender, RoutedEventArgs e)
        {
            var alarmID = ((Button)sender).Tag.ToString();
            IAlarm a = m_site.GetManager<IAlarmManager>().GetAlarmByResourceID(alarmID);
            if (a == null)
                return;

            foreach (IAttachment _attachment in a.Attachments)
            {
                if (_attachment is IVideoAttachment)
                {
                    IVideoAttachment vidAttach = _attachment as IVideoAttachment;
                    IMediaContentManager _mediaMgr = m_site.GetObject(ManagerIDConstants.MediaContentManagerID) as IMediaContentManager;
                    IMediaContent r = _mediaMgr.GetRecorded(vidAttach.Camera, vidAttach.Start, vidAttach.End);
                    if (r == null)
                        return;

                    Player play = new Player(vidAttach.Start, vidAttach.End, r);
                    play.Play();
                    play.Show();
                    break;
                }
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            //alarms.Clear();
            alarms.Insert(0, new Alarm()
            {
                Time = DateTime.Now.ToString("yyyy-MM-dd, HH:mm:ss"),
                AlarmID = "1000",
                Title = "test",
                Description = "test",
                State = "haha"
            });
        }

        private void Window_Closing_1(object sender, CancelEventArgs e)
        {
            try
            {
                this.CleanUp();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Callback of AlarmAdded event (IAlarmManger).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlarmRetrievalAndNotificationsSampleControl_AlarmAdded(object sender, AlarmEventArgs e)
        {
            foreach (IAlarm _alarm in e.Alarms)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    (Action<Alarm>)(alarm => { alarms.Insert(0, alarm);}),
                    new Alarm()
                    {
                        //Time = _alarm.Timestamp.ToLocalTime().ToString("yyyy-MM-dd, HH:mm:ss"),
                        Time = _alarm.Timestamp.ToLocalTime().ToString(@"MM/dd/yyyy HH:mm:ss"),
                        AlarmID = _alarm.ResourceID,
                        CameraName = " Camera 0001",
                        Location = " Platform",
                        Title = _alarm.Title,
                        Description = _alarm.Description,
                        State = _alarm.State.ToString() + " - " + (_alarm.IsClosed ? "Closed" : "Not Closed")
                    });
                    

                /*
                alarms.Insert(0, new Alarm() {
                    Time = _alarm.Timestamp.ToLocalTime().ToString("yyyy-MM-dd, HH:mm:ss"), 
                    AlarmID = _alarm.ResourceID, 
                    Title = _alarm.Title, 
                    Description = _alarm.Description, 
                    State = _alarm.State.ToString() + " - " + (_alarm.IsClosed ? "Closed" : "Not Closed")
                });
                */
                /*this.listView1.Items.Insert(0, new Alarm()
                {
                    Time = DateTime.Now.ToString("yyyy-MM-dd, HH:mm:ss"),
                    AlarmID = "1000",
                    Title = "test",
                    Description = "test",
                    State = "haha"
                });*/
            }
        }

        private void btnGetAll_Click(object sender, RoutedEventArgs e)
        {
            //btnGetAll.IsEnabled = false;
            DateTime _endTime = DateTime.Now;

            this.Cursor = Cursors.Wait;

            try
            {
                IAlarmFilter _filter = m_site.GetManager<IAlarmManager>().NewAlarmFilter();

                _filter.AddTimestampToFiltering(m_lastDateTime, _endTime);
                //_filter.AddAlarmSourceTypeToFiltering(AlarmSourceTypes.Unknown);
                //_filter.AddDescriptionTextToFiltering("SavVi", FilterStringSearchType.PartialStringMatch);
                _filter.AddTimestampToSorting(SortingDirections.Descendant);
                m_lastDateTime = _endTime;

                DataObjectCollection _doc = m_site.GetManager<IAlarmManager>().GetAlarms(_filter);

                foreach (IAlarm _alarm in _doc)
                {
                    Alarm alarm = new Alarm()
                    {
                        Time = _alarm.Timestamp.ToLocalTime().ToString(@"MM/dd/yyyy HH:mm:ss"),
                        AlarmID = _alarm.ResourceID,
                        CameraName = " Camera 0001",
                        Location = "  Platform",
                        Title = _alarm.Title,
                        Description = _alarm.Description,
                        State = _alarm.State.ToString() + " - " + (_alarm.IsClosed ? "Closed" : "Not Closed")
                    };
                    alarms.Add(alarm);
                    /*ListViewItem item = new ListViewItem();
                    item.DataContext = alarm;
                    this.listView1.Items.Add(item);*/
                }
            }
            catch (ClientSDKException ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }

            //btnGetAll.IsEnabled = true;
        }

        private void playAttachmentVideo(ICamera cam, DateTime start, DateTime end)
        {
            /*

            IMediaContentManager _mediaMgr = m_site.GetObject(ManagerIDConstants.MediaContentManagerID) as IMediaContentManager;
            IMediaContent r = _mediaMgr.GetRecorded(cam, start, end);
            if (r == null)
                return;

            m_videoCtrl.Open(r);
            m_videoCtrl.Play();
            */
        }

        private void showAttachmentShot(IImage image)
        {
            using (MemoryStream _stream = new MemoryStream(image.Content))
            {
                m_image.Image = System.Drawing.Image.FromStream(_stream);
            }

        }

        private void onSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            Alarm a = listView1.SelectedItem as Alarm;
            if(a == null)
                return;

            IAlarm _a = m_site.GetManager<IAlarmManager>().GetAlarmByResourceID(a.AlarmID);
            if (_a == null)
                return;

            foreach (IAttachment _attachment in _a.Attachments)
            {
                if (_attachment is IImageAttachment)
                {
                    IImageAttachment imageAttach = _attachment as IImageAttachment;
                    showAttachmentShot(imageAttach.Image);
                }
            }

            txtInfo.Text = a.Time;
            
        }

        private void SetScreenPosition()
        {
            if (placeScreenNumber <= ScreenNumber && ScreenNumber > 1)
            {
                if (WindowState == WindowState.Normal)
                {
                    Left += (placeScreenNumber - 1) * ScreenWidth;
                }
                else if (WindowState == WindowState.Maximized)
                {
                    Hide();
                    WindowState = WindowState.Normal;
                    Left = (placeScreenNumber - 1) * ScreenWidth - 7;
                    Application.Current.MainWindow.WindowState = WindowState.Maximized;
                    Show();
                }
            }
        }

        public void ParseCommandLine()
        {
            string[] args = Environment.GetCommandLineArgs();
            foreach (string arg in args)
            {
                try
                {
                    int index = arg.IndexOf('=');
                    //string[] sArray = strReadLine.Split('=');
                    string name = arg.Substring(0, index);  //sArray[0];
                    string value = arg.Substring(index + 1, arg.Length - index - 1); //sArray[1];
                    if (name == "screen")
                        placeScreenNumber = Convert.ToInt32(value);
                }
                catch (Exception)
                {
                }
            }
        }
    }

}
