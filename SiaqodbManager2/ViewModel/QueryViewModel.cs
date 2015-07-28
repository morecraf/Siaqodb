using SiaqodbManager.MacWinInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbManager.ViewModel
{
    public class QueryViewModel:INotifyPropertyChanged
    {
        private IDialogService saveFileDialog;
        private string linq;
        private string file;

        public QueryViewModel(IDialogService saveFile)
        {
            this.saveFileDialog = saveFile;
        }

        internal void Save(string file)
        {
            if (file == null)
            {
                file = saveFileDialog.OpenDialog();
                if (!String.IsNullOrEmpty(file))
                {
                    using (StreamWriter sw = new StreamWriter(file))
                    {
                        sw.Write(Linq);
                    }
                }
            }
            else
            {
                using (StreamWriter sw = new StreamWriter(file))
                {
                    sw.Write(Linq);
                }
            }
        }

        public string Linq
        {
            get
            {
                return linq;
            }
            set
            {
                linq = value;
                OnPropertyChanged();
            }
        }
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }


        public event PropertyChangedEventHandler PropertyChanged;


        internal void SaveAs()
        {
            string file = saveFileDialog.OpenDialog();
            if (!String.IsNullOrEmpty(file))
            {
                using (StreamWriter sw = new StreamWriter(file))
                {
                    sw.Write(linq);
                    this.file = file;
                }
            }
        }

        public string File
        {
            get { return file; }
            set
            {
                file = value;
                OnPropertyChanged();
            }
        }

        internal void Execute(string path)
        {
            //if (this.path != path)
            //{
            //    if (!System.IO.Directory.Exists(path))
            //    {
                    //  textBox1.Text = "Invalid folder! choose a valid database folder";
                    //  tabControl1.SelectedIndex = 1;
            //        return;
            //    }

            //    this.path = path;
            //}

          //  textBox1.Text = "";

            Sqo.SiaqodbConfigurator.EncryptedDatabase = false;

            Sqo.Siaqodb siaqodbConfig = new Sqo.Siaqodb(AppDomain.CurrentDomain.BaseDirectory + "\\config");
            Sqo.IObjectList<NamespaceItem> namespaces = siaqodbConfig.LoadAll<NamespaceItem>();
            Sqo.IObjectList<ReferenceItem> references = siaqodbConfig.LoadAll<ReferenceItem>();
            siaqodbConfig.Close();

            EncryptionViewModel.Instance.SetEncryptionSettings();//set back settings

            string ifEncrypted = @" Sqo.SiaqodbConfigurator.SetLicense(@"" qU3TtvA4T4L30VSlCCGUTbooYKG1XXCnjJ+jaPPrPLaD7PdPw9HujjxmkZ467OqZ"");";
            if (EncryptionViewModel.Instance.IsEncryptedChecked)
            {
                ifEncrypted += @" Sqo.SiaqodbConfigurator.EncryptedDatabase=true;
                                 Sqo.SiaqodbConfigurator.SetEncryptor(Sqo.BuildInAlgorithm." + EncryptionViewModel.Instance.Algorithm + @"); 

                                ";
                if (!string.IsNullOrEmpty(EncryptionViewModel.Instance.Pwd))
                {
                    ifEncrypted += @" SiaqodbConfigurator.SetEncryptionPassword(""" + EncryptionViewModel.Instance.Pwd + @""");";
                }
            }
#if TRIAL
            ifEncrypted += @" SiaqodbConfigurator.SetTrialLicense("""+TrialLicense.LicenseKey+@""");";
#endif
            string metBody = ifEncrypted + @" Siaqodb siaqodb = Sqo.Internal._bs._ofm(@""" + path + @""",""SiaqodbManager,SiaqodbManager2"");
			
							object list= (" + Linq + @").ToList();
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

                    IList w = ((IList)retVal);

                //    this.dataGridView1.DataSource = w;
                  //  this.dataGridView1.AutoGenerateColumns = true;
                   // this.tabControl1.SelectedIndex = 0;
                    //this.lblNrRows.Text = ar.Count + " rows";
                }
                catch (Exception ex)
                {
                    WriteErrors(ex.ToString());
              //      this.tabControl1.SelectedIndex = 1;
                }
            }
            else
            {
           //     this.tabControl1.SelectedIndex = 1;
            }

        }
        private void WriteErrors(string errorLine)
        {
            //Error text
          //  this.textBox1.Text += errorLine + "\r\n";
        }
    }
}
