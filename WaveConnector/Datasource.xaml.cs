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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Datasource : Window
    {
        private SFDC.SforceService SFDCBinding = null;
        public Datasource(SFDC.SforceService SFDCBinding)
        {
            this.SFDCBinding = SFDCBinding;
            InitializeComponent();
        }

        private void OData_Click(object sender, RoutedEventArgs e)
        {
            odataOK.Visibility = Visibility.Visible;
            btnOData.IsEnabled = false;

            odbcOK.Visibility = Visibility.Hidden;
            btnOdbc.IsEnabled = true;

            btnContinue.IsEnabled = true;

        }

        private void btnOdbc_Click(object sender, RoutedEventArgs e)
        {
            odataOK.Visibility = Visibility.Hidden;
            btnOData.IsEnabled = true;

            odbcOK.Visibility = Visibility.Visible;
            btnOdbc.IsEnabled = false;

            btnContinue.IsEnabled = true;
        }

        private void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            if(btnOData.IsEnabled == false)
            {
                ODataConf odataconfWin = new ODataConf(SFDCBinding);
                odataconfWin.WindowStartupLocation = WindowStartupLocation.Manual;
                odataconfWin.Top = this.Top;
                odataconfWin.Left = this.Top;
                odataconfWin.Show();
                this.Close();
            } 
            else if (btnOdbc.IsEnabled == false)
            {
                OdbcConf odbcconfWin = new OdbcConf(SFDCBinding);
                odbcconfWin.WindowStartupLocation = WindowStartupLocation.Manual;
                odbcconfWin.Top = this.Top;
                odbcconfWin.Left = this.Top;
                odbcconfWin.Show();
                this.Close();
            }  
        }
    }
}
