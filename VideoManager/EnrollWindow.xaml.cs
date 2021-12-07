using System;
using System.Collections.Generic;
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
using System.Runtime.InteropServices;
using Core;

namespace VideoManager
{
    /// <summary>
    /// Interaction logic for Facial.xaml
    /// </summary>
    public partial class EnrollWindow : Window
    {
        [DllImport("USER32.DLL", SetLastError = true)]
        public static extern int GetSystemMetrics(int nIndex);
        private uint ScreenWidth = (uint)GetSystemMetrics(0);
        private uint ScreenNumber = (uint)GetSystemMetrics(80);

        private bool bNameClicked = false;
        private bool bNoteClicked = false;

        public FacialWindow facialWindow = null;
        private string fileCapturedFullName = "";
        ICameraControl cameraControl;

        public EnrollWindow()
        {
            InitializeComponent();
            
            txtNumber.Text = (Core.DAIHelper.Instance.getLastId("template", "pkey") + 1).ToString();
            this.Loaded += new RoutedEventHandler(Window_Loaded_1);
        }

        //for page jump
        public void SetFacialWindow(FacialWindow facialWindow)
        {
            this.facialWindow = facialWindow;
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
            facialWindow.displayWindow.Show();
            facialWindow.displayWindow.Focus();
            StopCamera();
        }

        private void menuButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Button btn = (Button)sender;
            btn.Foreground = Brushes.Black;
        }

        private void menuButton_MouseLeave(object sender, MouseEventArgs e)
        {
            Button btn = (Button)sender;
            btn.Foreground = Brushes.White;
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
                cameraControl = new GeneralCameraControl();
            }

            cameraControl.Create(panel1);
            cameraControl.Initialize(CachedMap.Instance.cameraRefreshInterval, CachedMap.Instance.snapURL, CachedMap.Instance.videoSnapURL);
        }

        private void btnHomepage_Click(object sender, RoutedEventArgs e)
        {
            facialWindow.displayWindow.Show();
            facialWindow.displayWindow.Focus();
            StopCamera();
        }

        private void btnDetection_Click(object sender, RoutedEventArgs e)
        {
            if (facialWindow == null)
            {
                facialWindow = new FacialWindow();
                facialWindow.SetEnrollWindow(this);
            }
            facialWindow.Show();
            facialWindow.Focus();
            StopCamera();
        }

        //top menu, no need handle
        private void btnEnroll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnManage_Click(object sender, RoutedEventArgs e)
        {
            facialWindow.manageWindow.Show();
            facialWindow.manageWindow.Focus();
            StopCamera();
        }

        private void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            if (btnCapture.Content.ToString() == "CAPTURE")
            {
                cameraControl.Freeze();
                btnCapture.Content = "RESUME";
                string fileCaptured = CachedMap.Instance.folder_image_gallery + txtNumber.Text + ".jpg";
                if (cameraControl.Capture(fileCaptured) == true)
                {
                    fileCapturedFullName = fileCaptured;
                }
            }
            else
            {
                cameraControl.UnFreeze();
                btnCapture.Content = "CAPTURE";
                fileCapturedFullName = "";
            }
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            //cameraControl.Freeze();
            //Generate sql first
            if (txtName.Text == "")
            {
                MessageBox.Show("Please Enter the Name!");
                txtName.Focus();
            }
            else
            {
                RadioButton rb = infoPanel.Children.OfType<RadioButton>().FirstOrDefault(r => r.IsChecked == true);
                string sqlstr = "insert into template(pkey,name,type,note) values";
                sqlstr += "(" + txtNumber.Text + ",'" + txtName.Text + "','" + rb.Content.ToString() + "','" + txtNote.Text + "')";
                if(fileCapturedFullName != "")
                {
                    cameraControl.Stop();
                    btnCapture.Content = "CAPTURE";
                    //enroll to template
                    string an_image_to_enroll = fileCapturedFullName;
                    string a_template = CachedMap.Instance.folder_template_gallery + txtNumber.Text + ".out";
                    int iRet = CachedMap.Instance.pTfrs.EnrollOne(an_image_to_enroll, a_template);
                    //int iRet = CachedMap.Instance.pTfrs.EnrollOne("inPic.jpg", "inBmp.out");
                    //int iRet = CachedMap.Instance.pTfrs.EnrollOne("inBmp.bmp", "inBmp.out");
                    if (iRet != TFRSCli_.Tfrs_ERR_SUCCESS)
                    {
                        Console.WriteLine("error:pTfrs.EnrollOne() == " + iRet);
                        MessageBox.Show("Enroll failed, iRet = ." + iRet);
                        File.Delete(fileCapturedFullName);
                    }
                    else
                    {   //submit
                        Core.DAIHelper.Instance.ExecuteNonQuery(sqlstr);
                        CachedMap.Instance.ReloadTemplates();
                        CachedMap.Instance.pTfrs.AddTemplate(a_template);
                        facialWindow.manageWindow.ReloadTemplates();
                        MessageBox.Show("Submit Successfully.", "Display Manager");
                        txtNumber.Text = (Core.DAIHelper.Instance.getLastId("template", "pkey") + 1).ToString();
                        fileCapturedFullName = "";
                        //btnCapture.Content = "CAPTURE";
                    }
                }
                else
                {
                    MessageBox.Show("Cannot Capture Image, Please Start Camera First.", "Error");
                }
            }
            //cameraControl.UnFreeze();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            txtName.Text = "";
            txtNote.Text = "";
            rbVIP.IsChecked = true;
            rbBlackList.IsChecked = false;
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

        private void txtName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (bNameClicked == false)
            {
                TextBox t = (TextBox)sender;
                t.Text = "";
                bNameClicked = true;
            }
        }

        private void txtNote_GotFocus(object sender, RoutedEventArgs e)
        {
            if (bNoteClicked == false)
            {
                TextBox t = (TextBox)sender;
                t.Text = "";
                bNoteClicked = true;
            }
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
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

        private void btnCamera_Click(object sender, RoutedEventArgs e)
        {
            if (btnCamera.Content.ToString() == "START CAMERA")
            {
                StartCamera();
            }
            else if (btnCamera.Content.ToString() == "STOP")
            {
                StopCamera();
            }
        }

        public void StartCamera()
        {
            imgPortrait.Visibility = Visibility.Collapsed;
            cameraControl.SetVisibility(Visibility.Visible);
            cameraControl.Start();
            btnCamera.Content = "STOP";
            btnCapture.Content = "CAPTURE";
        }

        public void StopCamera()
        {
            cameraControl.Stop();
            btnCamera.Content = "START CAMERA";
            btnCapture.Content = "CAPTURE";
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".png";
            dlg.Filter = "JPG Files (*.jpg)|*.jpg|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|GIF Files (*.gif)|*.gif";
            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();
            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                StopCamera();
                cameraControl.SetVisibility(Visibility.Collapsed);
                imgPortrait.Visibility = Visibility.Visible;
                // Open document 
                string filename = dlg.FileName;
                fileCapturedFullName = CachedMap.Instance.folder_image_gallery + txtNumber.Text + ".jpg";
                File.Copy(filename, fileCapturedFullName, true);
                ShowPortrait(fileCapturedFullName);
            }
        }

        private void ShowPortrait(string strPhoto)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.UriSource = new Uri(strPhoto);
            image.EndInit();

            imgPortrait.Source = image;
        }

        private void Window_Deactivated_1(object sender, EventArgs e)
        {
            /*cameraControl.Stop();
            btnCamera.Content = "START CAMERA";
            btnCapture.Content = "CAPTURE";*/
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
