using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Media;

namespace VideoManager
{
    public class User
    {
        public int ID { get; set; }
        public string Photo { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Time { get; set; }
        public string Note { get; set; }
        public string CameraName { get; set; }
        public string RealPhoto { get; set; }
        //public ImageSource RealPhoto { get; set; }
    }
}
