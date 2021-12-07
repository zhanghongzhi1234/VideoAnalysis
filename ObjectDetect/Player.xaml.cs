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
using Verint.VideoSolutions.Business.Common;
using Verint.VideoSolutions.Business.Client;
using System.Windows.Threading;

namespace ObjectDetect
{
    /// <summary>
    /// Interaction logic for Player.xaml
    /// </summary>
    public partial class Player : Window
    {
        private NextivaVideoControl m_videoCtrl = new NextivaVideoControl();
        DateTime startTime;
        DateTime endTime;
        IMediaContent media;
        DateTime currentTime;
        TimeSpan totalTime;
        TimeSpan intervalTimeSpan;
        DispatcherTimer m_timer;
        int interval = 200;

        public Player(DateTime startTime, DateTime endTime, IMediaContent media)
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
            System.Windows.Forms.Integration.WindowsFormsHost hostPlayer = new System.Windows.Forms.Integration.WindowsFormsHost();
            hostPlayer.Child = m_videoCtrl;
            Grid1.Children.Add(hostPlayer);

            this.startTime = startTime;
            this.endTime = endTime;
            this.media = media;
            totalTime = endTime - startTime;
            intervalTimeSpan = TimeSpan.FromMilliseconds(Convert.ToDouble(interval));
            
            Reset();

            m_timer = new DispatcherTimer();
            m_timer.Interval = intervalTimeSpan;
            m_timer.Tick += new EventHandler(timer1_Tick);
        }
        
        public void Play()
        {
            if (media == null)
                return;

            m_videoCtrl.Open(media);
            m_videoCtrl.Play();
            m_timer.Start();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (btnStart.Content.ToString() == "PAUSE")
            {
                m_timer.Stop();
                m_videoCtrl.Pause();
                btnStart.Content = "PLAY";
            }
            else if (btnStart.Content.ToString() == "PLAY")
            {
                m_timer.Start();
                m_videoCtrl.Play();
                btnStart.Content = "PAUSE";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            slider1.Value += interval;
            if (slider1.Value >= slider1.Maximum)
            {
                Reset();
            }
            Step();
        }

        private void Reset()
        {
            slider1.Minimum = 0;
            slider1.Maximum = totalTime.TotalMilliseconds;
            slider1.Value = 0;
            currentTime = startTime;
            try
            {
                m_videoCtrl.Stop();
                m_videoCtrl.Close();
                m_videoCtrl.Open(media);
                m_videoCtrl.Play();
            }
            catch(Exception)
            {

            }
            txtElapsedTime.Text = "00:00 / " + totalTime.ToString(@"mm\:ss");
        }

        private void Step()
        {
            slider1.Value += interval;
            currentTime += intervalTimeSpan;
            txtCurrentTime.Text = currentTime.ToLocalTime().ToString(@"M/d/yyyy HH:mm:ss");
            TimeSpan elapsedTime = TimeSpan.FromMilliseconds(Convert.ToDouble(slider1.Value));
            //TimeSpan elapsedTime = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(slider1.Value));
            txtElapsedTime.Text = elapsedTime.ToString(@"mm\:ss") + " / " + totalTime.ToString(@"mm\:ss");
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
    }
}
