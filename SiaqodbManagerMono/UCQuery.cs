using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Reflection;
using System.IO;

namespace SiaqodbManager
{
	public partial class UCQuery : UserControl
	{
		public UCQuery()
		{
			InitializeComponent();
		}
		string path;
		
		public void Initialize(string path)
		{
			string appPath = Path.GetDirectoryName(Application.ExecutablePath);
		
		}

		private void button1_Click(object sender, EventArgs e)
		{
		}
		private string file;
		public void Save()
		{
			if (this.file == null)
			{
				SaveFileDialog sfd = new SaveFileDialog();
				sfd.DefaultExt = ".linq";
				sfd.Filter = "(*.linq)|*.linq|All Files(*.*)|*.*";
				DialogResult dg = sfd.ShowDialog();
				if (dg == DialogResult.OK)
				{

					using (StreamWriter sw = new StreamWriter(sfd.FileName))
					{
						sw.Write(this.textEditorControl1.Text);
						this.file = sfd.FileName;
					}
				}
			}
			else
			{
				using (StreamWriter sw = new StreamWriter(this.file))
				{
					sw.Write(this.textEditorControl1.Text);
				}
			}
		}
		public void SaveAs()
		{

			SaveFileDialog sfd = new SaveFileDialog();
			sfd.DefaultExt = ".linq";
			sfd.Filter = "(*.linq)|*.linq|All Files(*.*)|*.*";
			DialogResult dg = sfd.ShowDialog();
			if (dg == DialogResult.OK)
			{

				using (StreamWriter sw = new StreamWriter(sfd.FileName))
				{
					sw.Write(this.textEditorControl1.Text);
					this.file = sfd.FileName;
				}
			}

		}


        public void Execute(string path)
        {
            if (this.path != path)
            {
                if (!System.IO.Directory.Exists(path))
                {
                    textBox1.Text = "Invalid folder! choose a valid database folder";
                    tabControl1.SelectedIndex = 1;
                    return;
                }

                this.path = path;
            }

            textBox1.Text = "";

            Sqo.SiaqodbConfigurator.EncryptedDatabase = false;

            Sqo.Siaqodb siaqodbConfig = new Sqo.Siaqodb(Application.StartupPath);
            Sqo.IObjectList<NamespaceItem> namespaces = siaqodbConfig.LoadAll<NamespaceItem>();
            Sqo.IObjectList<ReferenceItem> references = siaqodbConfig.LoadAll<ReferenceItem>();
            siaqodbConfig.Close();

            EncryptionSettings.SetEncryptionSettings();//set back settings

            string ifEncrypted = "";
            if (EncryptionSettings.IsEncryptedChecked)
            {
                ifEncrypted = @" SiaqodbConfigurator.EncryptedDatabase=true;
                                 SiaqodbConfigurator.SetEncryptor(BuildInAlgorithm." + EncryptionSettings.Algorithm + @"); 

                                ";
                if (!string.IsNullOrEmpty(EncryptionSettings.Pwd))
                {
                    ifEncrypted += @"SiaqodbConfigurator.SetEncryptionPassword(" + EncryptionSettings.Pwd + ");";

                }
            }
#if TRIAL
            ifEncrypted += @" SiaqodbConfigurator.SetTrialLicense("""+TrialLicense.LicenseKey+@""");";
#endif
            string metBody = ifEncrypted + @" Siaqodb siaqodb = Sqo.Internal._bs._ofm(@""" + this.path + @""",""SiaqodbManager,SiaqodbManager2"");
			
							object list= (" + this.textEditorControl1.Text + @").ToList();
                            siaqodb.Close();
                            return list;
							 ";
            var c = new CodeDom();
            //c.AddReference(@"System.Core.dll");
            //c.AddReference(@"siaqodb.dll");
            //c.AddReference(@"System.Windows.Forms.dll");


            foreach (ReferenceItem refi in references)
            {
                c.AddReference(refi.Item);
            }
            System.CodeDom.CodeNamespace n = c.AddNamespace("LINQQuery");
            foreach (NamespaceItem nitem in namespaces)
            {
                n.Imports(nitem.Item);
            }
            n.Imports("System.Collections.Generic")
            .Imports("System.Linq")
            .Imports("Sqo")



            .AddClass(
              c.Class("RunQuery")
                .AddMethod(c.Method("object", "FilterByLINQ", "", metBody)));

            Assembly assembly = c.Compile(WriteErrors);
            if (assembly != null)
            {
                Type t = assembly.GetType("LINQQuery.RunQuery");
                MethodInfo method = t.GetMethod("FilterByLINQ");

                try
                {
                    var retVal = method.Invoke(null, null);
                    //Type[] tt = retVal.GetType().GetGenericArguments();
                    IList w = ((IList)retVal);
                    //ArrayList ar = new ArrayList();
                    //while (w.MoveNext())
                    //{
                    //    ar.Add(w.Current);

                    //}
                    this.dataGridView1.DataSource = w;
                    this.dataGridView1.AutoGenerateColumns = true;
                    this.tabControl1.SelectedIndex = 0;
                    //this.lblNrRows.Text = ar.Count + " rows";
                }
                catch (Exception ex)
                {
                    WriteErrors(ex.ToString());
                    this.tabControl1.SelectedIndex = 1;
                }
            }
            else
            {
                this.tabControl1.SelectedIndex = 1;
            }

        }
		private void WriteErrors(string errorLine)
		{
			this.textBox1.Text += errorLine + "\r\n";
		}
		public string GetFile()
		{
			return file;
		}

		internal void SetText(string s,string file)
		{
			this.textEditorControl1.Text = s;
			this.file = file;
		}
	}
}
