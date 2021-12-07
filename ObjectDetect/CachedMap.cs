using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
//using log4net;
using System.Windows.Forms;

namespace ObjectDetect
{
    public sealed class CachedMap : IDisposable
    {
        private static volatile CachedMap instance;        //singleton
        private static object syncRoot = new Object();
        
        //public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, string> runParams = new Dictionary<string, string>();
        public int placeScreenNumber = 1;
        public string crowdURL = "";
        public bool showCrowd = true;
        public int screenX = 0;
        public int screenY = 0;

        private CachedMap() 
        {
            ParseRunParamFile();
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

        /// <summary>
        /// A method for disposing TFRSCli_ object
        /// </summary>
        public void Dispose()
        {
        }
    }
}
