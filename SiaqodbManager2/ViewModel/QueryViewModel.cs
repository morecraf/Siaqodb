using SiaqodbManager.Entities;
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
using MonoMac.AppKit;

namespace SiaqodbManager.ViewModel
{
    public class QueryViewModel:INotifyPropertyChanged
    {
        private IDialogService saveFileDialog;

		private string linq = "from Customer c in siaqodb select c";

        private string file;


        public QueryViewModel(IDialogService saveLinqService, MainViewModel mainViewModel)
        {

            this.saveFileDialog = saveLinqService;
            this.Parent = mainViewModel;

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

        internal void Execute()
        {

            if (!System.IO.Directory.Exists(Path))
            {
				OnErrorOccured ("Invalid folder, choose a valid database folder");
                return;
            }

            //textBox1.Text = "";

			Sqo.SiaqodbConfigurator.EncryptedDatabase = false;

			Sqo.Siaqodb siaqodbConfig = new Sqo.Siaqodb(AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar +"config");
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
			string metBody = ifEncrypted + @"
							 Siaqodb siaqodb = null;
							 try{
							    siaqodb = Sqo.Internal._bs._ofm(@""" + Path + @""",""SiaqodbManager,SiaqodbManager2"");
				
								object list= (" + Linq + @").ToList();
								return list;
                            }finally{
								if(siaqodb != null){
									siaqodb.Close();
								}
							}
							 ";
            var c = new CodeDom();
			c.AddReference(@"System.Core.dll"); 
			c.AddReference(@"System.dll");
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
			n.Imports ("System.Collections.Generic")
            .Imports ("System.Linq")
            .Imports ("Sqo")

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

                    OnLinqExecuted(w);
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

        private void OnLinqExecuted(IList dataSource)
        {
            if (LinqExecuted != null)
            {
                LinqExecuted(this, new LinqEventArgs
                {
                    DataSource = dataSource
                });
            }
        }

        private void OnErrorOccured(string errorLine)
        {
            if (ErrorOccured != null)
            {
                ErrorOccured(this, new ErrorMessageArgs
                {
                    Message = errorLine
                });
            }
        }

        public EventHandler<LinqEventArgs> LinqExecuted;
        public EventHandler<ErrorMessageArgs> ErrorOccured;
        private MainViewModel Parent;

	

        private void WriteErrors(string errorLine)
        {
            OnErrorOccured(errorLine);
        }

        public string Path
        {
            get
            {
                return Parent.SelectedPath.Item;
            }
        }
    }
}
