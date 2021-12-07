using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace VideoManager
{
    public interface ICameraControl
    {
        void Create(Panel panel, bool strech = false);
        void Initialize(int interval, string snapURL, string videoSnapURL);
        void Start();
        void Stop();
        void Freeze();
        void UnFreeze();
        bool Capture(string filePath);
        Bitmap Capture();
        void SetVisibility(Visibility visibility);
        void CleanUp();
    }
}
