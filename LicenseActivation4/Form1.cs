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
                MessageBox.Show("Please enter your Customer Code");
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            try
            {
                string k = GetLicenseKey(this.txtNetCustomerCode.Text);
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

        public static string GetLicenseKey(string customerCode)
        {
            string hostname = Environment.MachineName;
            WebRequest request = WebRequest.Create(@"https://siaqodb.com/licensor/licensorv40.php?c=" + customerCode + "&m=" + hostname + "&l=1");
            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            response.Close();
            return responseFromServer;
        }
    }
}
