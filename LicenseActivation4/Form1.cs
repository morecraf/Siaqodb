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

        private void btnGetLicenseKey_Click(object sender, EventArgs e)
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
                    this.btnSave.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.Cursor = Cursors.Default;
            
        }
        
       
        private void btnSave_Click(object sender, EventArgs e)
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
        #region SILVERLIGHT
        private void btnGetKeySilv_Click(object sender, EventArgs e)
        {
            if (txtCodeSilv.Text == string.Empty)
            {
                MessageBox.Show("Fill CustomerCode sent by email by Siaqodb");
                return;
            }
            if (txtAssembly.Text == string.Empty)
            {
                MessageBox.Show("Load an Silverlight assembly!");
                return;
            }
          
          
            try
            {
                Assembly a = Assembly.LoadFile(txtAssembly.Text);
                if (a != null)
                {
                    var mscorlib = a.GetReferencedAssemblies().FirstOrDefault(ae => string.Compare(ae.Name, "mscorlib", true) == 0); 
        
                    ulong token = BitConverter.ToUInt64(mscorlib.GetPublicKeyToken(), 0);
                    if (token == 0x8e79a7bed785ec7c)//SL
                    { 
                        
                    }
                    else if (token == 0x89e03419565c7ab7)//FRW
                    { 
                    
                    }
                    object[] at = a.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
                    if (at.Length > 0)
                    {
                        string guid = ((System.Runtime.InteropServices.GuidAttribute)at[0]).Value;
                        string ass = a.FullName.Split(',')[0];
                        this.Cursor = Cursors.WaitCursor;
                        string k = "";
                        try
                        {
                            k = sla4.SilvLicActivator.GetSilverlightLicenseKey(guid, ass, txtCodeSilv.Text);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        this.Cursor = Cursors.Default;
                        this.txtLicSilv.Text = k;
                        if (!k.Trim().StartsWith("ERR"))
                        {
                            this.btnSaveSilv.Enabled = true;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Silverlight assembly!");
                }
            }
            catch
            {
                MessageBox.Show("Invalid Silverlight assembly!");
              
            }
            
        }
       

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "Silverlight assembly (*.dll)|*.dll";
            opf.InitialDirectory = Application.StartupPath;
            opf.Multiselect = false;
            if (opf.ShowDialog() == DialogResult.OK)
            {
                this.txtAssembly.Text = opf.FileName;
            }

        }

        private void btnSaveSilv_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "siaqodb.lic";
            sfd.DefaultExt = ".lic";
            sfd.Filter = "(*.lic)|*.lic|All Files(*.*)|*.*";
            DialogResult dg = sfd.ShowDialog();
            if (dg == DialogResult.OK)
            {

                using (StreamWriter sw = new StreamWriter(sfd.FileName))
                {
                    sw.Write(this.txtLicSilv.Text);

                }
            }
        }
        
        #endregion
    }
}
