using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sqo;

namespace SiaqodbManager
{
    public partial class TrialLicenseFrm : Form
    {
        public TrialLicenseFrm()
        {
            InitializeComponent();
        }
        string licenseKey;
       
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                SiaqodbConfigurator.SetTrialLicense(this.textBox1.Text);
                Sqo.Siaqodb siaqodbConfig = new Sqo.Siaqodb(Application.StartupPath);
                siaqodbConfig.Close();
                TrialLicense.LicenseKey = textBox1.Text;
                this.licenseKey = textBox1.Text;
                this.DialogResult = DialogResult.OK;

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        internal string GetLicenseKey()
        {
            return this.licenseKey;
        }
    }
    public class TrialLicense
    {
        public static string LicenseKey;
    }
}
