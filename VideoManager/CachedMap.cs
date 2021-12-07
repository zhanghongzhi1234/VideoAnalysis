using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using TFRSCli;
using TFRSUtil;
using Core;
//using log4net;
using System.Windows.Forms;

namespace VideoManager
{
    public sealed class CachedMap : IDisposable
    {
        private static volatile CachedMap instance;        //singleton
        private static object syncRoot = new Object();
        
        public string folder_image_gallery;
        public string folder_template_gallery;
        public string folder_image_probe;
        public string configPath;
        public int numCandidates;
        public int numMaxFaces;
        public TFRSUtil_ pTfrs = null;
        public Dictionary<int, Template> templateMap = new Dictionary<int, Template>();
        public string cameraType = "";
        public string snapURL = "";
        public string videoSnapURL = "";
        public int cameraRefreshInterval = 80;
        public string noPortraint = "";
        public double score = 0d;
        //public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, string> runParams = new Dictionary<string, string>();
        public int placeScreenNumber = 1;
        public string crowdURL = "";
        public bool showCrowd = true;
        public int screenX = 0;
        public int screenY = 0;

        private CachedMap() 
        {
            //FileInfo configFile = new FileInfo("log4net.config");
            //log4net.Config.XmlConfigurator.ConfigureAndWatch(configFile);
            ParseRunParamFile();
            ParseCommandLine();
            cameraType = GetRunParam("Camera1Type");
            snapURL = GetRunParam("Camera11SnapURL");
            videoSnapURL = GetRunParam("Camera1VideoSnapURL");
            cameraRefreshInterval = Convert.ToInt32(GetRunParam("CameraRefreshInterval"));
            score = Convert.ToDouble(GetRunParam("Score"));
            crowdURL = GetRunParam("CrowdURL");
            if (GetRunParam("ShowCrowd") != "1")
                showCrowd = false;
            if(isSetRunParam("screen"))
                placeScreenNumber = Convert.ToInt32(GetRunParam("screen"));
            if (isSetRunParam("screenX"))
                screenX = Convert.ToInt32(GetRunParam("screenX"));
            if (isSetRunParam("screenY"))
                screenY = Convert.ToInt32(GetRunParam("screenY"));
            if (cameraType == "Bosch" || cameraType == "VLC" || cameraType == "General")           //need change in future if allow 32 bit and 64 bit sdk run together
            {
                templateMap = Core.DAIHelper.Instance.GetAllTemplates();

                folder_image_gallery = Environment.CurrentDirectory + "/image_gallery/";
                folder_template_gallery = Environment.CurrentDirectory + "/template_gallery/";
                folder_image_probe = Environment.CurrentDirectory + "/image_probe/";
                configPath = "c:\\TOSHIBA\\SDK_FRVT2013_Final\\SDK\\dic\\facedata";
                numCandidates = 10;
                Console.WriteLine("configPath," + configPath);
                numMaxFaces = 10;
                try
                {
                    InitToshibaSDK();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    //MessageBox.Show(ex.ToString());
                }
                noPortraint = Environment.CurrentDirectory + "/images/noPortrait.png";
            }
            else
            {
                InitVMS();
            }
        }

        public void ResetToshibaSDK()
        {
            pTfrs.Dispose();
            InitToshibaSDK();
        }

        private void InitToshibaSDK()
        {
            pTfrs = new TFRSUtil_(configPath);
            LoadToshibaTemplates();
        }
        
        //make sure to call ReloadTemplates before load template to sdk
        private void LoadToshibaTemplates()
        {
            if (pTfrs.isInitialized == true)
            {
                VerifyTemplate();
                int iRet = TFRSCli_.Tfrs_ERR_SUCCESS;
                iRet = pTfrs.LoadTemplates(folder_template_gallery);
                if (TFRSCli_.Tfrs_ERR_SUCCESS != iRet)
                {
                    DebugUtil.Instance.LOG.Info("Cannot Load Templates");
                    MessageBox.Show("Cannot Load Templates!", "Display Manager");
                }
                else
                {
                    DebugUtil.Instance.LOG.Info("Load template successfully");
                }
            }
            else
            {
                MessageBox.Show("Sdk not initialized, " + (CachedMap.Instance.pTfrs.noLicense == true ? "invalid License!" : "!"), "Video Manager");
            }
        }

        //check if db templates and template file are exactly mapping, if not, make them map
        private void VerifyTemplate()
        {
            List<int> listA = new List<int>(templateMap.Keys);
            List<int> listB = new List<int>();
            List<string> listBToBeDelete = new List<string>();
            DirectoryInfo d = new DirectoryInfo(folder_template_gallery);//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.out"); //Getting Text files
            foreach (FileInfo file in Files)
            {
                try
                {
                    int temp = Convert.ToInt32(Path.GetFileNameWithoutExtension(file.FullName));
                    listB.Add(temp);
                }
                catch (Exception)
                {
                    //File.Delete(file.FullName);
                    listBToBeDelete.Add(file.FullName);
                }
            }
            foreach (string item in listBToBeDelete)
            {
                File.Delete(item);
            }
            listA.Sort();
            listB.Sort();
            bool equal = listA.SequenceEqual(listB);
            if (equal == false)
            {
                MessageBox.Show("Template file and database not match, please check the config!", "Error");
            }
            //listA.Where(a => !listB.Any(a.SequenceEqual)).Union(
            //listB.Where(b => !listA.Any(b.SequenceEqual))).ToList();
        }

        private bool InitVMS()
        {
            bool ret = false;
            /*m_site = new NextivaSite();
            string server = "192.168.253.200";
            string port = "5005";
            string userName = "Administrator";
            string password = "cctvware";

            try
            {
                server = GetRunParam("VMSServerIP");
                port = GetRunParam("VMSServerPort");
                userName = GetRunParam("VMSServerUsername");
                password = GetRunParam("VMSServerPassword");
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
                bool retry = true;
                while (retry)
                {
                    try
                    {
                        m_site.LoginEx(userName, password);
                        retry = false;
                    }
                    catch (Exception e)
                    {
                        DialogResult res = System.Windows.Forms.MessageBox.Show(e.Message, "Connection failed...", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                        if (res == DialogResult.Retry)
                        {
                            retry = true;
                        }
                        else
                        {
                            retry = false;
                            ret = false;
                        }

                    }
                }
            }*/
            return ret;
        }

        public static CachedMap Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new CachedMap();
                    }
                }

                return instance;
            }
        }

        
        /// <summary>
        /// A method for disposing TFRSCli_ object
        /// </summary>
        public void Dispose()
        {
            pTfrs.Dispose();
        }

        public void ReloadTemplates()
        {
            templateMap.Clear();
            templateMap = Core.DAIHelper.Instance.GetAllTemplates();
        }

        public void ParseRunParamFile()
        {
            runParams = Core.DAIHelper.Instance.ReadConfigFile();
        }

        /** Add a parameter
		  * @param name Name of parameter
          * @param value Value of parameter
          * Pre: name and value are not NULL
		  */
        public void SetRunParam(string name, string value)
        {
            runParams[name] = value;
        }

        /**Retrieve a parameter value
          * @return Value of parameter
		  * @param name Name of parameter
          * Pre: name is not NULL
          */
        public string GetRunParam(string name)
        {
            return runParams[name];
        }

		/** Determine whether a parameter with the given name has been set
          * @return True (parameter set), False (parameter not set)
		  * @param name Name of parameter
          * Pre: name is not NULL
          */
        bool isSetRunParam(string name)
        {
            bool isSet = runParams.ContainsKey(name);
            return isSet;
        }

        public void ParseCommandLine()
        {
            string[] args = Environment.GetCommandLineArgs();
            bool bFoundX = false;
            bool bFoundY = false;
            string value = "1";
            int screenX = 0;
            int screenY = 0;
            foreach (string arg in args)
            {
                try
                {
                    DebugUtil.Instance.LOG.Info(arg);
                    if (bFoundX == true)
                    {
                        string temp = arg.Trim('\'');
                        screenX = Convert.ToInt32(temp);
                        runParams["screenX"] = screenX.ToString();
                        DebugUtil.Instance.LOG.Info("screenX:" + screenX);
                        bFoundX = false;
                    }
                    if (arg == "-screenX")
                        bFoundX = true;

                    if (bFoundY == true)
                    {
                        string temp = arg.Trim('\'');
                        screenY = Convert.ToInt32(temp);
                        runParams["screen"] = value;
                        runParams["screenY"] = screenY.ToString();
                        DebugUtil.Instance.LOG.Info("screenY:" + screenY);
                        bFoundY = false;
                    }
                    if (arg == "-screenY")
                        bFoundY = true;
                }
                catch(Exception)
                {
                }
            }

            if ((screenX == 0 && screenY == 0) || System.Windows.Forms.Screen.AllScreens.Count() == 1)
                value = "1";
            else
                value = "2";

            runParams["screen"] = value;
            DebugUtil.Instance.LOG.Info("screen=" + value);
        }
    }
}
