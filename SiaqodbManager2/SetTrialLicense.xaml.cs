using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Sqo;

namespace SiaqodbManager
{
    /// <summary>
    /// Interaction logic for SetTrialLicense.xaml
    /// </summary>
    public partial class SetTrialLicense : Window
    {
        public SetTrialLicense()
        {
            InitializeComponent();
        }
        string licenseKey;
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SiaqodbConfigurator.SetLicense(this.textBox1.Text);
                Sqo.Siaqodb siaqodbConfig = new Sqo.Siaqodb(AppDomain.CurrentDomain.BaseDirectory);
                siaqodbConfig.Close();
                TrialLicense.LicenseKey = textBox1.Text;
                this.licenseKey = textBox1.Text;
                this.DialogResult = true;

                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

        }

        internal string GetLicenseKey()
        {
            return this.licenseKey;
        }
    }
}
