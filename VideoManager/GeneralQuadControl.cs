using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows.Controls;

namespace VideoManager
{
    public class GeneralQuadControl : IQuadControl
    {
        System.Windows.Forms.Integration.WindowsFormsHost cameraHost1 = new System.Windows.Forms.Integration.WindowsFormsHost();
        System.Windows.Forms.Integration.WindowsFormsHost cameraHost2 = new System.Windows.Forms.Integration.WindowsFormsHost();
        System.Windows.Forms.Integration.WindowsFormsHost cameraHost3 = new System.Windows.Forms.Integration.WindowsFormsHost();
        System.Windows.Forms.Integration.WindowsFormsHost cameraHost4 = new System.Windows.Forms.Integration.WindowsFormsHost();
        string snapURL1 = "";
        string snapURL2 = "";
        string snapURL3 = "";
        string snapURL4 = "";
        PictureBox pictureBox1 = new PictureBox();
        PictureBox pictureBox2 = new PictureBox();
        PictureBox pictureBox3 = new PictureBox();
        PictureBox pictureBox4 = new PictureBox();
        bool showImage1 = false;
        bool showImage2 = false;
        bool showImage3 = false;
        bool showImage4 = false;
        private DispatcherTimer m_timer1;

        public void Create(System.Windows.Controls.Panel panel1, System.Windows.Controls.Panel panel2, System.Windows.Controls.Panel panel3, System.Windows.Controls.Panel panel4, bool strech = false)
        {
            CreateQuadItem(1, panel1, cameraHost1, pictureBox1, strech);
            CreateQuadItem(2, panel2, cameraHost2, pictureBox2, strech);
            CreateQuadItem(3, panel3, cameraHost3, pictureBox3, strech);
            CreateQuadItem(4, panel4, cameraHost4, pictureBox4, strech);
        }

        private void CreateQuadItem(int index, System.Windows.Controls.Panel panel, System.Windows.Forms.Integration.WindowsFormsHost cameraHost, PictureBox pictureBox, bool strech = false)
        {
            //cameraHost = new System.Windows.Forms.Integration.WindowsFormsHost();
            //pictureBox = new PictureBox();
            pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            if (strech == false)
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            else
                pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            // Assign the ActiveX control as the host control's child.
            cameraHost.Child = pictureBox;
            //pictureBox1.BackColor = System.Drawing.Color.Yellow;
            panel.Children.Add(cameraHost);
            System.Windows.Forms.ContextMenu contextmenu = new System.Windows.Forms.ContextMenu();
            pictureBox.ContextMenu = contextmenu;
            // create label
            System.Windows.Forms.Label label = new System.Windows.Forms.Label();
            label.BackColor = Color.Transparent;
            label.Font = new Font("Calibri", 20, System.Drawing.FontStyle.Regular);
            int X = Convert.ToInt32(panel.ActualWidth / 2) - 50;
            int Y = Convert.ToInt32(panel.ActualHeight / 2) - 20;
            label.Size = new System.Drawing.Size(200, 50);
            label.Location = new System.Drawing.Point(X, Y);
            label.Text = "Display " + index;
            pictureBox.Controls.Add(label);
            System.Windows.Forms.MenuItem mi = new System.Windows.Forms.MenuItem();
            mi.Text = "Clear Image";
            mi.Tag = index;
            contextmenu.MenuItems.Add(mi);
            mi.Click += new EventHandler(ImageClear_Click);
            //cameraHost.Visibility = Visibility.Hidden;            //don't change cameraHost visibility, don't know why the first panel become black background when clear
        }

        public void Initialize()
        {
            snapURL1 = snapURL2 = snapURL3 = snapURL4 = CachedMap.Instance.videoSnapURL;

            m_timer1 = new DispatcherTimer();
            m_timer1.Interval = TimeSpan.FromMilliseconds(CachedMap.Instance.cameraRefreshInterval);
            m_timer1.Tick += new EventHandler(timer1_Tick);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (showImage1 == true)
            {
                pictureBox1.Load(snapURL1);
            }
            if (showImage2 == true)
            {
                pictureBox2.Load(snapURL1);
            }
            if (showImage3 == true)
            {
                pictureBox3.Load(snapURL1);
            }
            if (showImage4 == true)
            {
                pictureBox4.Load(snapURL1);
            }
        }

        private void SetQuadStatus(bool newStatus, ref bool showImage, PictureBox pictureBox, System.Windows.Forms.Integration.WindowsFormsHost cameraHost)
        {
            if (showImage == true && newStatus == false)
            {
                pictureBox.Image = null;
                showImage = false;
                pictureBox.Controls[0].Visible = true;     // show the label
            }
            else if (showImage == false && newStatus == true)
            {
                showImage = true;
                pictureBox1.Controls[0].Visible = false;     // show the label
            }
        }

        //set input parameter to false to stop the quad item
        public void Start(bool quad1, bool quad2, bool quad3, bool quad4)
        {
            SetQuadStatus(quad1, ref showImage1, pictureBox1, cameraHost1);
            SetQuadStatus(quad2, ref showImage2, pictureBox2, cameraHost2);
            SetQuadStatus(quad3, ref showImage3, pictureBox3, cameraHost3);
            SetQuadStatus(quad4, ref showImage4, pictureBox4, cameraHost4);

            if (showImage1 || showImage2 || showImage3 || showImage4 == true)
            {
                m_timer1.Start();
            }
            else if (showImage1 && showImage2 && showImage3 && showImage4 == false)
            {
                m_timer1.Stop();
            }
        }

        public void Start(int index)
        {
            if (index == 1)
            {
                showImage1 = true;
                pictureBox1.Controls[0].Visible = false;
            }
            if (index == 2)
            {
                showImage2 = true;
                pictureBox2.Controls[0].Visible = false;
            }
            if (index == 3)
            {
                showImage3 = true;
                pictureBox3.Controls[0].Visible = false;
            }
            if (index == 4)
            {
                showImage4 = true;
                pictureBox4.Controls[0].Visible = false;
            }

            if (showImage1 || showImage2 || showImage3 || showImage4 == true)
            {
                m_timer1.Start();
            }
        }

        void ImageClear_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MenuItem item = (System.Windows.Forms.MenuItem)sender;
            int index = (int)item.Tag;
            if (index == 1 && showImage1 == true)
            {
                pictureBox1.Image = null;
                showImage1 = false;
                pictureBox1.Controls[0].Visible = true;     // show the label
            }
            else if (index == 2 && showImage2 == true)
            {
                pictureBox2.Image = null;
                showImage2 = false;
                pictureBox2.Controls[0].Visible = true;
            }
            else if (index == 3 && showImage3 == true)
            {
                pictureBox3.Image = null;
                showImage3 = false;
                pictureBox3.Controls[0].Visible = true;
            }
            if (index == 4 && showImage4 == true)
            {
                pictureBox4.Image = null;
                showImage4 = false;
                pictureBox4.Controls[0].Visible = true;
            }
            if (showImage1 && showImage2 && showImage3 && showImage4 == false)
            {
                m_timer1.Stop();
            }
        }

        public void CleanUp()
        {

        }
    }
}
