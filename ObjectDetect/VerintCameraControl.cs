using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Verint.VideoSolutions.Business.Common;
using Verint.VideoSolutions.Business.Client;
using System.Windows.Forms;

namespace ObjectDetect
{
    public class VerintCameraControl : ICameraControl
    {
        System.Windows.Forms.Integration.WindowsFormsHost cameraHost;
        private NextivaVideoControl m_axCameo = null;

        public NextivaSite m_site;
        /*private LoginModes m_loginMode = LoginModes.Unknown;
        string server = "192.168.253.200";
        string port = "5005";
        string userName = "Administrator";
        string password = "cctvware";*/

        ICamera camera;
        string cameraName = "Camera 0001";

        public void Create(System.Windows.Controls.Panel panel)
        {
            cameraHost = new System.Windows.Forms.Integration.WindowsFormsHost();

            // Create the ActiveX control.
            m_axCameo = new Verint.VideoSolutions.Business.Client.NextivaVideoControl();
            m_axCameo.Dock = System.Windows.Forms.DockStyle.Fill;

            // Assign the ActiveX control as the host control's child.
            cameraHost.Child = m_axCameo;
            panel.Children.Add(cameraHost);
        }

        public void Initialize(object server)
        {
            m_site = (NextivaSite)server;
            try
            {
                // Retrieving the all the cameras configured in the system.
                ICameraManager cameraManager = m_site.GetObject(ManagerIDConstants.CameraManagerID) as ICameraManager;
                camera = cameraManager.GetCameraByName(cameraName);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message);
            }
        }

        public void Start()
        {
            IMediaContentManager _mediaMgr = m_site.GetObject(ManagerIDConstants.MediaContentManagerID) as IMediaContentManager;

            // 1. Use the MediaContentManager to retrieve live media content 
            //    according to the selected camera.
            IMediaContent _mediaContent = _mediaMgr.GetLive(camera);

            // 2. Load the live media content into the video control.
            m_axCameo.Open(_mediaContent);

            // 3. Start streaming live video.
            m_axCameo.Play();			
        }

        public void Stop()
        {
            m_axCameo.Close();
        }

        public void Freeze()
        {
            m_axCameo.Stop();
        }

        public void UnFreeze()
        {
            m_axCameo.Play();
        }

        public bool Capture(string filePath)
        {
            m_axCameo.SaveImage(filePath);
            return true;
        }

        public void SetVisibility(Visibility visibility)
        {
            cameraHost.Visibility = visibility;
        }

        public void CleanUp()
        {

        }
    }
}
