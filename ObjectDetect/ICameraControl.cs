using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ObjectDetect
{
    public interface ICameraControl
    {
        void Create(System.Windows.Controls.Panel panel);
        void Initialize(object server);
        void Start();
        void Stop();
        void Freeze();
        void UnFreeze();
        bool Capture(string filePath);
        void SetVisibility(Visibility visibility);
        void CleanUp();
    }
}
