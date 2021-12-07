using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace VideoManager
{
    public class GeneralCameraControl : ICameraControl
    {
        System.Windows.Forms.Integration.WindowsFormsHost cameraHost;
        string snapURL = "";
        string videoSnapURL = "";
        PictureBox pictureBox1;
        private DispatcherTimer m_timer1;
        WebClient wc;

        public void Create(System.Windows.Controls.Panel panel, bool strech = false)
        {
            cameraHost = new System.Windows.Forms.Integration.WindowsFormsHost();

            // Create the ActiveX control.
            pictureBox1 = new PictureBox();
            pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            if (strech == false)
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            else
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            // Assign the ActiveX control as the host control's child.
            cameraHost.Child = pictureBox1;
            //pictureBox1.BackColor = System.Drawing.Color.Yellow;
            panel.Children.Add(cameraHost);
            ContextMenu contextmenu = new ContextMenu();
            pictureBox1.ContextMenu = contextmenu;
            MenuItem mi = new MenuItem();
            mi.Text = "Clear Image";
            contextmenu.MenuItems.Add(mi);
            mi.Click += new EventHandler(ImageClear_Click);

            wc = new WebClient();
        }

        public void Initialize(int interval, string snapURL, string videoSnapURL)
        {
            this.snapURL = snapURL;
            this.videoSnapURL = videoSnapURL;

            m_timer1 = new DispatcherTimer();
            m_timer1.Interval = TimeSpan.FromMilliseconds(interval);
            m_timer1.Tick += new EventHandler(timer1_Tick);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Load(videoSnapURL);
        }

        public void Start()
        {
            m_timer1.Start();		
        }

        public void Stop()
        {
            m_timer1.Stop();
            pictureBox1.Image = null;
        }

        public void Freeze()
        {
            m_timer1.Stop();
        }

        public void UnFreeze()
        {
            m_timer1.Start();
        }

        public bool Capture(string filePath)
        {
            Bitmap bmp = Capture();
            bmp.Save(filePath);
            return true;
        }

        public Bitmap Capture()
        {
            Bitmap bmpRet = null;
            try
            {
                Stream s = wc.OpenRead(snapURL);
                bmpRet = new Bitmap(s);
                s.Dispose();
            }
            catch(Exception e)
            {
            }

            return bmpRet;
        }

        public void SetVisibility(Visibility visibility)
        {
            cameraHost.Visibility = visibility;
        }

        private void ImageClear_Click(object sender, EventArgs e)
        {
            Stop();
        }

        public void CleanUp()
        {

        }
    }
}
