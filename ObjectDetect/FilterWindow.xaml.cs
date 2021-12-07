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
    public partial class FilterWindow : Window
    {
        NextivaSite m_site;
        IAlarmFilter _filter;
        DateTime startTime;
        DateTime endTime;
        string content;

        public FilterWindow(NextivaSite site)
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();

            m_site = site;
            _filter = m_site.GetManager<IAlarmManager>().NewAlarmFilter();

           // _filter.AddTimestampToFiltering(m_lastDateTime, _endTime);
            //_filter.AddAlarmSourceTypeToFiltering(AlarmSourceTypes.Unknown);
            //_filter.AddDescriptionTextToFiltering("SavVi", FilterStringSearchType.PartialStringMatch);
            //_filter.AddTimestampToSorting(SortingDirections.Descendant);
            //m_lastDateTime = _endTime;
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
