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
    public partial class CameraView : Window
    {
        NextivaSite m_site;
        ICameraControl camera;

        public CameraView(NextivaSite site)
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();

            m_site = site;
            camera = new VerintCameraControl();
            camera.Create(grid1);
            camera.Initialize(m_site);
            camera.Start();
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            camera.Stop();
            camera.CleanUp();
        }
    }
}
