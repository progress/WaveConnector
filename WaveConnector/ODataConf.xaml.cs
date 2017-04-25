using System.Windows;
using System.Net;
using System;
using Simple.OData.Client;
using System.Windows.Media;


namespace WaveConnector
{
    /// <summary>
    /// Interaction logic for ODataConf.xaml
    /// </summary>
    public partial class ODataConf : Window
    {
        private SFDC.SforceService SFDCBinding;
        private ODataClientSettings settings;
        private ODataClient client;
        public ODataConf(SFDC.SforceService SFDCBinding)
        {

            InitializeComponent();
            this.SFDCBinding = SFDCBinding;
        }

        private void rbBAuth_Checked(object sender, RoutedEventArgs e)
        {
            txtODataUserName.IsEnabled = true;
            txtODataPassword.IsEnabled = true;
            btnTest.IsEnabled = true;
        }

        private void rbNAuth_Checked(object sender, RoutedEventArgs e)
        {
            txtODataUserName.IsEnabled = false;
            txtODataPassword.IsEnabled = false;
            btnTest.IsEnabled = true;
        }

        private async void btnTest_Clicked(object sender, RoutedEventArgs e)
        {
            string OdataURI = null;
            if(String.IsNullOrWhiteSpace(txtODataURI.Text))
            {
                MessageBox.Show("OData URI cannot be empty");
                return;
            }

            if(txtODataURI.Text.EndsWith("/"))
            {
                int length = txtODataURI.Text.Length;
                OdataURI = txtODataURI.Text.Substring(0, length - 1);
            }
            else
            {
                OdataURI = txtODataURI.Text;
            }

            settings = new ODataClientSettings();
            settings.BaseUri = new System.Uri(OdataURI);
            if (rbBAuth.IsChecked == true)
            {
                if (!String.IsNullOrWhiteSpace(txtODataUserName.Text) && !String.IsNullOrWhiteSpace(txtODataPassword.Password))
                {
                    settings.Credentials = new NetworkCredential(txtODataUserName.Text, txtODataPassword.Password);
                }
                else
                {
                    MessageBox.Show("Username or Password cannot be empty");
                    return;
                }
            }
            client = new ODataClient(settings);
            try
            {
                var response = await client.GetMetadataAsync<Microsoft.Data.Edm.IEdmModel>();

                btnContinue.IsEnabled = true;
                lblstatus.Content = "Test connection successful!";
                lblstatus.FontWeight = FontWeights.Medium;
                lblstatus.Foreground = Brushes.Green;
                lblstatus.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if(ex.Message == "Not Found")
                {
                    lblstatus.Content = "Invalid OData endpoint";
                    lblstatus.FontWeight = FontWeights.Medium;
                    lblstatus.Foreground = Brushes.Red;
                    lblstatus.Visibility = Visibility.Visible;
                }
                else if(ex.Message == "Unauthorized")
                {
                    lblstatus.Content = "Invalid Credentials";
                    lblstatus.FontWeight = FontWeights.Medium;
                    lblstatus.Foreground = Brushes.Red;
                    lblstatus.Visibility = Visibility.Visible;
                }
            }

        }

        private void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            ODataEntitySelect odataEntityWin = new ODataEntitySelect(settings, client, SFDCBinding);
            odataEntityWin.WindowStartupLocation = WindowStartupLocation.Manual;
            odataEntityWin.Top = this.Top;
            odataEntityWin.Left = this.Top;
            odataEntityWin.Show();
            this.Close();
        }

        private void btnD2C_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.progress.com/cloud-data-integration");
        }
    }
}
