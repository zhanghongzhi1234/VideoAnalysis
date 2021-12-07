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
using System.Collections;
using System.Runtime.InteropServices;

namespace VideoManager
{
    /// <summary>
    /// Interaction logic for Facial.xaml
    /// </summary>
    public partial class ManageWindow : Window
    {
        [DllImport("USER32.DLL", SetLastError = true)]
        public static extern int GetSystemMetrics(int nIndex);
        private uint ScreenWidth = (uint)GetSystemMetrics(0);
        private uint ScreenNumber = (uint)GetSystemMetrics(80);

        public FacialWindow facialWindow = null;
        ObservableCollection<User> users = new ObservableCollection<User>();
        ICameraControl cameraControl;

        public ManageWindow()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(Window_Loaded_1);
            ReloadTemplates();
            listView1.ItemsSource = users;

        }

        public void ReloadTemplates()
        {
            users.Clear();
            foreach (KeyValuePair<int, Template> entry in CachedMap.Instance.templateMap)
            {
                string strPhoto = CachedMap.Instance.folder_image_gallery + entry.Key.ToString() + ".jpg";
                if (!File.Exists(strPhoto))
                    strPhoto = CachedMap.Instance.noPortraint;
                users.Add(new User() { ID = entry.Key, Photo = strPhoto, Name = entry.Value.name, Type = entry.Value.type, Note = entry.Value.note });
            }
            if (listView1.Items.Count > 0)
                listView1.SelectedIndex = 0;
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

        private void btnHomePage_Click(object sender, RoutedEventArgs e)
        {
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
        }

        /// <summary>
        /// A method for matching M probes to N galleries in 1toN manner
        /// </summary>
        /// <returns>error code from TFRSCli_ object</returns>
        public int OneToN(string inimage)
        {
            int iRet = TFRSCli_.Tfrs_ERR_SUCCESS;

            List<string> result_list = new List<string>();

            Bitmap inbmp = new Bitmap(inimage);

            iRet = CachedMap.Instance.pTfrs.OneToN(inbmp, CachedMap.Instance.numCandidates, ref result_list);

            inbmp.Dispose();

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
                if (score > 0.65)
                {
                    Template temp = CachedMap.Instance.templateMap[ID];
                    string strPhoto = CachedMap.Instance.folder_image_gallery + temp.pkey.ToString() + ".jpg";
                    users.Add(new User() { ID = temp.pkey, Photo = strPhoto, Name = temp.name, Type = temp.type, Note = temp.note + score.ToString() });
                }
            }
            //ICollectionView view = CollectionViewSource.GetDefaultView(users);
            //view.Refresh();
            return iRet;
        }

        private void btnHomepage_Click(object sender, RoutedEventArgs e)
        {
            facialWindow.displayWindow.Show();
            facialWindow.displayWindow.Focus();
        }

        private void btnDetection_Click(object sender, RoutedEventArgs e)
        {
            facialWindow.Show();
            facialWindow.Focus();
        }

        private void btnEnroll_Click(object sender, RoutedEventArgs e)
        {
            facialWindow.enrollWindow.Show();
            facialWindow.enrollWindow.Focus();
        }

        private void btnManage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnReCapture_Click(object sender, RoutedEventArgs e)
        {
            if (btnReCapture.Content.ToString() == "RECAPTURE")
            {
                //m_cameo.FreezeFrame(true, true);
                btnReCapture.Content = "SUBMIT";
            }
            else
            {
                //m_cameo.FreezeFrame(false, false);
                btnReCapture.Content = "RECAPTURE";
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //IEnumerable<CheckBox> query = infoPanel.Children.OfType<CheckBox>().Where(r => r.IsChecked == true);
            //bool isChecked = false;
            
            ArrayList keyList = new ArrayList();
            //ArrayList fileImages = new ArrayList();
            //ArrayList fileTemplates = new ArrayList();
            //int i = 0;
            foreach (CheckBox item in FindVisualChildren<CheckBox>(listView1))
            {
                if (item.IsChecked == true)
                {
                    //isChecked = true;
                    keyList.Add(item.Tag.ToString());
                    
                    //strsql += item.Tag + ",";
                    //fileImages.Add(CachedMap.Instance.folder_image_gallery + item.Tag + ".jpg");
                    //fileTemplates.Add(CachedMap.Instance.folder_template_gallery + item.Tag + ".out");
                    //i++;
                }
            }
            if (keyList.Count > 0)
            {
                string sMessageBoxText = "Do you want to delete the selected template? It will take a while to rebuild the whole template library.";
                string sCaption = "Display Manager";
                MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
                MessageBoxImage icnMessageBox = MessageBoxImage.Warning;
                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
                if (rsltMessageBox == MessageBoxResult.Yes)
                {
                    //users.Clear();
                    //return;
                    //delete file
                    string strsql = "delete from template where pkey in(";
                    foreach (string item in keyList)
                    {
                        strsql += item + ",";
                        string fileImage = CachedMap.Instance.folder_image_gallery + item + ".jpg";
                        File.Delete(fileImage);
                        string fileTemplate = CachedMap.Instance.folder_template_gallery + item + ".out";
                        File.Delete(fileTemplate);
                        //CachedMap.Instance.templateMap.Remove(Convert.ToInt32(item));     //put code below
                    }
                    /*foreach (string item in fileImages)
                    {
                        File.Delete(item);
                    }
                    foreach (string item in fileTemplates)
                    {
                        File.Delete(item);
                    }*/
                    //delete from db
                    strsql = strsql.Trim(',');
                    strsql += ")";
                    Core.DAIHelper.Instance.ExecuteNonQuery(strsql);

                    CachedMap.Instance.ReloadTemplates();
                    CachedMap.Instance.ResetToshibaSDK();
                    this.ReloadTemplates();
                    //facialWindow.ReloadTemplates();
                    MessageBox.Show("Delete Successfully!", "Delete Record");
                }
            }
            else
            {
                MessageBox.Show("Please select at least one item to delete!", "Delete Record");
            }
            
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
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

        private void listView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listView1.SelectedIndex < 0)
                return;
            Type t = listView1.SelectedItem.GetType();
            System.Reflection.PropertyInfo[] props = t.GetProperties();
            string propertyValue = props[0].GetValue(listView1.SelectedItem, null).ToString();
            //string strPhoto = CachedMap.Instance.folder_image_gallery + propertyValue + ".jpg";
            string strPhoto = CachedMap.Instance.folder_image_gallery + propertyValue + ".jpg";
            if(!File.Exists(strPhoto))
                strPhoto = CachedMap.Instance.noPortraint;

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(strPhoto);
            image.EndInit();

            imgPortrait.Source = image;
            //ShowImage(strPhoto);
        }

        private void ShowImage(string path)
        {
            string strUri2 = String.Format(@"pack://application:,,,/MyAssembly;component/resources/main titles/{0}", path);
            Stream iconStream2 = App.GetResourceStream(new Uri(strUri2)).Stream;
            imgPortrait.Source = returnImage(iconStream2);
        }

        public static BitmapImage returnImage(Stream iconStream)
        {
            Bitmap brush = new Bitmap(iconStream);
            System.Drawing.Image img = brush.GetThumbnailImage(brush.Height, brush.Width, null, System.IntPtr.Zero);
            var imgbrush = new BitmapImage();
            imgbrush.BeginInit();
            imgbrush.StreamSource = ConvertImageToMemoryStream(img);
            imgbrush.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            imgbrush.EndInit();
            var ib = new ImageBrush(imgbrush);
            return imgbrush;
        }

        public static MemoryStream ConvertImageToMemoryStream(System.Drawing.Image img)
        {
            var ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms;
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
