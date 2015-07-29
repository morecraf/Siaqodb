using SiaqodbManager.MacWinInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbManager.ViewModel
{
    class ReferencesViewModel: INotifyPropertyChanged
    {
        private string namespaceText;
        IDialogService fileDialog;

        internal List<ReferenceItem> assemblies = new List<ReferenceItem>();
        internal List<NamespaceItem> namespaces = new List<NamespaceItem>();

        public ReferencesViewModel(IDialogService fileDialog)
        {
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "config"))
            {
                this.fileDialog = fileDialog;
                References = new ObservableCollection<ReferenceItem>();
                AddStandardCommand = new MyCommand<object>(OnAddStandard);
                AddCommand = new MyCommand<object>(OnAddRef);
                RemoveCommand = new MyCommand<object>(OnRemoveRef);
                LoadReferencesCommand = new MyCommand<object>(OnLoadReferences);

                Sqo.SiaqodbConfigurator.EncryptedDatabase = false;
                Sqo.Siaqodb siaqodb = new Sqo.Siaqodb(AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "config");
                try
                {
                    Sqo.IObjectList<ReferenceItem> references = siaqodb.LoadAll<ReferenceItem>();
                    foreach (ReferenceItem refItem in references)
                    {
                        References.Add(refItem);
                    }
                    Sqo.IObjectList<NamespaceItem> namespacesItems = siaqodb.LoadAll<NamespaceItem>();
                    foreach (NamespaceItem nItem in namespacesItems)
                    {
                        Namespaces += nItem + Environment.NewLine;
                    }
                }
                finally
                {
                    siaqodb.Close();
                    EncryptionViewModel.Instance.SetEncryptionSettings();//set back settings
                }
            }
        }



        public ObservableCollection<ReferenceItem> References { get; set; }
        private ReferenceItem selectedRef;


        public string Namespaces
        {
            get
            {
                return namespaceText;
            }
            set
            {
                namespaceText = value;
                OnPropertyChanged();
            }
        }

        public ReferenceItem SelectedRef
        {
            get
            {
                return selectedRef;
            }
            set
            {
                selectedRef = value;
                OnPropertyChanged();
            }
        }

        public MyCommand<object> AddCommand { get; set; }
        public MyCommand<object> RemoveCommand { get; set; }
        public MyCommand<object> LoadReferencesCommand { get; set; }
        public MyCommand<object> AddStandardCommand { get; set; }
  
        public void OnLoadReferences(object obj){
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "config"))
            {
                assemblies.Clear();
                namespaces.Clear();
                Sqo.SiaqodbConfigurator.EncryptedDatabase = false;
                Sqo.Siaqodb siaqodb = new Sqo.Siaqodb(AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "config");
                try
                {
                    siaqodb.DropType<ReferenceItem>();
                    siaqodb.DropType<NamespaceItem>();
                    foreach (object o in References)
                    {
                        ReferenceItem refItem = o as ReferenceItem;
                        if (refItem == null)
                        {
                            refItem = new ReferenceItem(o.ToString());
                        }
                        assemblies.Add(refItem);
                        siaqodb.StoreObject(refItem);

                        if (File.Exists(refItem.Item))
                        {
                            try
                            {
                                File.Copy(refItem.Item, AppDomain.CurrentDomain.BaseDirectory + "\\" + System.IO.Path.GetFileName(refItem.Item), true);


                            }
                            catch
                            {

                            }
                        }
                    }
                    foreach (string s in Namespaces.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        NamespaceItem nobj = new NamespaceItem(s);
                        namespaces.Add(nobj);
                        siaqodb.StoreObject(nobj);
                    }
                }
                finally
                {
                    siaqodb.Close();
                    // EncryptionSettings.SetEncryptionSettings();
                }
            }

           // this.DialogResult = true;
        }
        public void OnAddRef(object obj)
        {
            var reference = fileDialog.OpenDialog();
            References.Add(new ReferenceItem
            {
                Item = reference
            });
        }
        public void OnRemoveRef(object obj)
        {
            if (SelectedRef != null)
            {
                References.Remove(SelectedRef);
            }
        }

        public void OnAddStandard(object obj)
        {
            References.Add(new ReferenceItem{ Item = "System.dll"});
            References.Add(new ReferenceItem { Item = "System.Core.dll" });
            References.Add(new ReferenceItem { Item ="System.Windows.Forms.dll"});
            References.Add(new ReferenceItem { Item = "siaqodb.dll" });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private DialogService.ReferenceFileService referenceFileService;

        //EVENT HANDLER
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
