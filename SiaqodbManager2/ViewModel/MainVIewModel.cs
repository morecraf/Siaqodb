using Sqo;
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
    class MainViewModel : INotifyPropertyChanged
    {
        private bool infoEnabled;
        private bool saveEnabled;
        private bool startEnabled;
        private ConnectionItem selectedPath;
        private bool executeEnabled;
        private ObjectViewModel objectsTable;
        private Sqo.Siaqodb siaqodb;


        public MainViewModel()
        {
            if (!System.IO.Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "config"))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "config");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }
            Sqo.SiaqodbConfigurator.SetLicense(@"jExqyPv94eVIquUhx6JU0jnAADpup2ullr3yN34pExs=");
#if TRIAL
            string folder = AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "config";
            string trialFile = folder + System.IO.Path.DirectorySeparatorChar + "trial.lic";
            if (System.IO.File.Exists(trialFile))
            {
                string text = System.IO.File.ReadAllText(trialFile);
                try
                {

                    SiaqodbConfigurator.SetLicense(text);
                    Sqo.Siaqodb siaqodbConfigTemp = new Sqo.Siaqodb(AppDomain.CurrentDomain.BaseDirectory);
                    siaqodbConfigTemp.Close();
                    TrialLicense.LicenseKey = text;
                }
                catch (Sqo.Exceptions.InvalidLicenseException ex)
                {
                    MessageBox.Show(ex.Message);
                    this.Close();
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    SetTrialLicense trialWnd = new SetTrialLicense();
                    if (trialWnd.ShowDialog() == true)
                    {
                        string trialKey = trialWnd.GetLicenseKey();
                        System.IO.File.WriteAllText(trialFile, trialKey);
                    }
                    else
                    {
                        this.Close();
                        return;
                    }
                }
            }
            else
            {
                SetTrialLicense trialWnd = new SetTrialLicense();
                if (trialWnd.ShowDialog() == true)
                {
                    string trialKey = trialWnd.GetLicenseKey();
                    System.IO.File.WriteAllText(trialFile, trialKey);
                }
                else
                {
                    this.Close();
                    return;
                }
            }
#endif

            Sqo.Siaqodb siaqodbConfig = new Sqo.Siaqodb(AppDomain.CurrentDomain.BaseDirectory);


            List = new ObservableCollection<ConnectionItem>(siaqodbConfig.LoadAll<ConnectionItem>());

            siaqodbConfig.Close();

            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "config"))
            {
                Sqo.Siaqodb siaqodbRef = new Sqo.Siaqodb(AppDomain.CurrentDomain.BaseDirectory + "\\config");

                Sqo.IObjectList<ReferenceItem> references = siaqodbRef.LoadAll<ReferenceItem>();
                foreach (ReferenceItem refi in references)
                {
                    if (File.Exists(refi.Item))
                    {
                        try
                        {
                            File.Copy(refi.Item, AppDomain.CurrentDomain.BaseDirectory + "\\" + System.IO.Path.GetFileName(refi.Item), true);
                        }
                        catch
                        {

                        }
                    }
                }

                siaqodbRef.Close();
                ExecuteEnabled = false;
               // menuExecute.IsEnabled = false;
                SaveEnabled = false;
                //menuSave.IsEnabled = false;
                //menuSaveAs.IsEnabled = false;
                InfoEnabled = false;
                ConnectCommand = new MyCommand<object>(OnConnect);

                TypesList = new ObservableCollection<MetaTypeViewModel>();
                Sqo.SiaqodbConfigurator.EncryptedDatabase = true;
            }
        }

        private void OnConnect(object obj)
        {
            if (System.IO.Directory.Exists(SelectedPath.Item))
            {
                if (!List.Contains(SelectedPath))
                {
                    List.Add(SelectedPath);
                    SiaqodbConfigurator.EncryptedDatabase = false;
                    Sqo.Siaqodb siaqodbConfig = new Sqo.Siaqodb(AppDomain.CurrentDomain.BaseDirectory);

                    siaqodbConfig.StoreObject(SelectedPath);
                    siaqodbConfig.Close();
                    EncryptionSettings.SetEncryptionSettings();//set back settings
                }
                siaqodb = Sqo.Internal._bs._b(SelectedPath.Item);

                var allMetaTypes = siaqodb.GetAllTypes().Select(m=>new MetaTypeViewModel(m));
                TypesList = new ObservableCollection<MetaTypeViewModel>(allMetaTypes);
                OnPropertyChanged("TypesList");
                //treeView1.Items.Clear();
                //ContextMenu cm = new ContextMenu();
                //MenuItem mitem = new MenuItem();
                //mitem.Header = "Load objects";
                //cm.Items.Add(mitem);
                //mitem.Click += new RoutedEventHandler(mitem_Click);
                //foreach (Sqo.MetaType mt in siaqodbList)
                //{

                //    //Sqo.Internal._bs._sdbfn(siaqodb, mt, mt.FileName);
                //    ImageTreeViewItem nodeType = new ImageTreeViewItem();
                //    nodeType.Tag = mt;
                //    nodeType.Text = mt.Name;
                //    nodeType.SelectedImage = ImageTreeViewItem.Createimage(@"pack://application:,,/Resources/pubclass.gif");
                //    nodeType.UnselectedImage = ImageTreeViewItem.Createimage(@"pack://application:,,/Resources/pubclass.gif");
                //    nodeType.ContextMenu = cm;

                //    treeView1.Items.Add(nodeType);
                    //foreach (Sqo.MetaField mf in mt.Fields)
                    //{
                    //    ImageTreeViewItem nodeField = new ImageTreeViewItem();
                    //    //nodeField.Header = mf.Name + "(" + mf.FieldType.ToString() + ")";
                    //    if (mf.FieldType != null)
                    //    {
                    //        nodeField.Text = mf.Name + "(" + mf.FieldType.ToString() + ")";
                    //    }
                    //    else
                    //    {
                    //        nodeField.Text = mf.Name + "(ComplexType)";
                    //    }
                    //    nodeField.SelectedImage = ImageTreeViewItem.Createimage(@"pack://application:,,/Resources/pubfield.gif");
                    //    nodeField.UnselectedImage = ImageTreeViewItem.Createimage(@"pack://application:,,/Resources/pubfield.gif");
                    //    nodeField.ContextMenu = null;
                    //    nodeType.Items.Add(nodeField);

                    //}

               // }
                InfoEnabled = true;
            }
            else
            {
              //  MessageBox.Show("Select a valid folder path!");
            }
        }

        internal void OnObjectLoad(MetaTypeViewModel SelectedType)
        {
            ObjectsTable = new ObjectViewModel(SelectedType,TypesList,siaqodb);
            SaveEnabled = false;
            ExecuteEnabled = false;
            
        }

        public MyCommand<object> ConnectCommand {get;set;}

        public ConnectionItem SelectedPath {
            get { return selectedPath; }
            set { 
                selectedPath = value;
                OnPropertyChanged(); 
            }
        }

        public bool InfoEnabled {
            get{
                return infoEnabled;
            }
            set
            {
                infoEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool StartEnabled
        {
            get
            {
                return startEnabled;
            }
            set
            {
                startEnabled = true;
                OnPropertyChanged();
            }
        }

        public bool SaveEnabled
        {
            get
            {
                return saveEnabled;
            }
            set
            {
                saveEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool ExecuteEnabled
        {
            get
            {
                return executeEnabled;
            }
            set
            {
                executeEnabled = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ConnectionItem> List { get; set; }
        public ObservableCollection<MetaTypeViewModel> TypesList { get; set; }
        public ObjectViewModel ObjectsTable
        {
            get
            {
                return objectsTable;
            }
            set
            {
                objectsTable = value;
                OnPropertyChanged();
            }
        }

        //EVENT HANDLER
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }


        public event PropertyChangedEventHandler PropertyChanged;


        public Siaqodb Siaqodb
        {
            get
            {
                return siaqodb;
            }
            set
            {
                siaqodb = value;
                OnPropertyChanged();
            }
        }
    }
}
