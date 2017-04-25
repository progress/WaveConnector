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

namespace WaveConnector
{
    /// <summary>
    /// Interaction logic for OdbcConf.xaml
    /// </summary>
    public partial class OdbcConf : Window
    {
        private SFDC.SforceService SFDCBinding = null;
        public OdbcConf(SFDC.SforceService SFDCBinding)
        {
            InitializeComponent();
            this.SFDCBinding = SFDCBinding;
        }
    }
}
