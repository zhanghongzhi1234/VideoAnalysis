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
    public interface IQuadControl
    {
        void Create(Panel panel1, Panel panel2, Panel panel3, Panel panel4, bool strech = false);
        void Initialize();
        void Start(bool quad1, bool quad2, bool quad3, bool quad4);
        void Start(int index);      //index is 1 to 4
        void CleanUp();
    }
}
