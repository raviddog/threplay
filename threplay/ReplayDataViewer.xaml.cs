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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace threplay
{
    /// <summary>
    /// Interaction logic for ReplayDataViewer.xaml
    /// </summary>
    public partial class ReplayDataViewer : UserControl
    {
        public ReplayDataViewer()
        {
            InitializeComponent();
        }

        ReplayEntry.ReplayInfo info { get; set; }
    }
}
