using System;
using System.Windows;
using WaveConnector.SFDC;
using System.Net;

namespace WaveConnector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            string user = txtUserName.Text;
            string key  = txtpassword.Password;
            string token = txtSecToken.Text;

            if(string.IsNullOrWhiteSpace(user))
            {
                MessageBox.Show("Username is empty");
                return;
            }
            else if(string.IsNullOrWhiteSpace(key))
            {
                MessageBox.Show("Password is empty");
                return;
            }
            else if(string.IsNullOrWhiteSpace(token))
            {
                MessageBox.Show("Security Token is empty");
                return;
            }

            key = key + token;

            //Authentication With Salesforce
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            SforceService SfdcBinding = null;
            LoginResult CurrentLoginResult = null;
            SfdcBinding = new SforceService();
            try
            {
                CurrentLoginResult = SfdcBinding.login(user, key);
                SfdcBinding.Url = CurrentLoginResult.serverUrl;
                SfdcBinding.SessionHeaderValue = new SessionHeader();
                SfdcBinding.SessionHeaderValue.sessionId = CurrentLoginResult.sessionId;

                //Open Next Form on Success
                ODataConf odataconf = new ODataConf(SfdcBinding);
                //Datasource datasourceWin = new Datasource(SfdcBinding);
                odataconf.WindowStartupLocation = WindowStartupLocation.Manual;
                odataconf.Top = this.Top;
                odataconf.Left = this.Top;
                odataconf.Show();
                this.Close();
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                SfdcBinding = null;
                MessageBox.Show("Invalid Credentials:" + ex.Message);
            }
            catch (Exception ex)
            {
                SfdcBinding = null;
                MessageBox.Show("Error: " + ex.Message);
            }
        }

    }
}
