using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;

namespace LicenseActivation
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
    
        private void btnGetLicenseKey_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtNetCustomerCode.Text))
            {
                MessageBox.Show("Fill CustomerCode");
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            try
            {
                string k = sla4.SilvLicActivator.GetLicenseKey(this.txtNetCustomerCode.Text);
                this.txtLic.Text = k;
                if (!k.Trim().StartsWith("ERR"))
                {
                    txtCode.Text = @"Sqo.SiaqodbConfigurator.SetLicense(@"""+k+@""");";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.Cursor = Cursors.Default;
            
        }

        private void btnSave_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".lic";
            sfd.FileName = "siaqodb.lic";
            sfd.Filter = "(*.lic)|*.lic|All Files(*.*)|*.*";
            DialogResult dg = sfd.ShowDialog();
            if (dg == DialogResult.OK)
            {

                using (StreamWriter sw = new StreamWriter(sfd.FileName))
                {
                    sw.Write(this.txtLic.Text);

                }
            }
        }
    }
}
