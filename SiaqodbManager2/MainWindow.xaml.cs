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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using AvalonDock;
using Sqo;
using System.IO;
using SiaqodbManager.Helpers;
using System.Diagnostics;
namespace SiaqodbManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Sqo.Siaqodb siaqodb;
        public static RoutedCommand execCommand = new RoutedCommand();
        public static RoutedCommand saveCommand = new RoutedCommand();
        public static RoutedCommand newCommand = new RoutedCommand();
        public static RoutedCommand openCommand = new RoutedCommand();
        
        public MainWindow()
        {
           
            InitializeComponent();
            InputBinding ib = new InputBinding(execCommand,new KeyGesture(Key.F5));
            this.InputBindings.Add(ib);

            CommandBinding cb = new CommandBinding(execCommand);
            cb.Executed += new ExecutedRoutedEventHandler(execCommand_Executed);
            this.CommandBindings.Add(cb);

            InputBinding ib1 = new InputBinding(saveCommand, new KeyGesture(Key.S,ModifierKeys.Control));
            this.InputBindings.Add(ib1);

            CommandBinding cb1 = new CommandBinding(saveCommand);
            cb1.Executed += new ExecutedRoutedEventHandler(saveCommand_Executed);
            this.CommandBindings.Add(cb1);

            InputBinding ib2 = new InputBinding(newCommand, new KeyGesture(Key.N, ModifierKeys.Control));
            this.InputBindings.Add(ib2);

            CommandBinding cb2 = new CommandBinding(newCommand);
            cb2.Executed += new ExecutedRoutedEventHandler(newCommand_Executed);
            this.CommandBindings.Add(cb2);

            InputBinding ib3 = new InputBinding(openCommand, new KeyGesture(Key.O, ModifierKeys.Control));
            this.InputBindings.Add(ib3);

            CommandBinding cb3 = new CommandBinding(openCommand);
            cb3.Executed += new ExecutedRoutedEventHandler(openCommand_Executed);
            this.CommandBindings.Add(cb3);

          
            
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.IO.Directory.Exists(App.ConfigDbPath))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(App.ConfigDbPath);
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }
            Sqo.SiaqodbConfigurator.SetLicense(@" qU3TtvA4T4L30VSlCCGUTQ9J7I0xVsr5/glyn/JzNY4yVy640fieqvjNywXzj9og");


            using (Sqo.Siaqodb siaqodbConfig = new Sqo.Siaqodb(App.ConfigDbPath))
            {
                IObjectList<ConnectionItem> list = siaqodbConfig.LoadAll<ConnectionItem>();

                foreach (ConnectionItem item in list)
                {
                    cmbDBPath.Items.Add(item.Item);
                }
               
            }


            DefaultDocument uq = new DefaultDocument();
            uq.Title = "Start";
            SetDefaultSettings(uq);
            uq.Show(this.dockingManager1);
            uq.Activate();

            dockingManager1.ActiveDocumentChanged += new EventHandler(dockingManager1_ActiveDocumentChanged);
            dockingManager1.DocumentClosing += new EventHandler<System.ComponentModel.CancelEventArgs>(dockingManager1_DocumentClosing);
            btnExecute.IsEnabled = false;
            menuExecute.IsEnabled = false;
            btnSave.IsEnabled = false;
            menuSave.IsEnabled = false;
            menuSaveAs.IsEnabled = false;
            btnDBInfo.IsEnabled = false;

        }

        void dockingManager1_DocumentClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (prev != null)
            {
                if (prev.IsLoaded)
                {
                    prev.Activate();
                }
               
            }
        }

        private ManagedContent current;
        private ManagedContent prev;
        void dockingManager1_ActiveDocumentChanged(object sender, EventArgs e)
        {
            if (dockingManager1.ActiveDocument != null)
            {
                prev = current;
                current = dockingManager1.ActiveDocument;
                
            }
            QueryDocument query = dockingManager1.ActiveDocument as QueryDocument;
            
            if (query != null)
            {
                btnExecute.IsEnabled = true;
                menuExecute.IsEnabled = true;
                btnSave.IsEnabled = true;
                menuSave.IsEnabled = true;
                menuSaveAs.IsEnabled = true;
            }
            else 
            {
                btnExecute.IsEnabled = false;
                menuExecute.IsEnabled = false;
                btnSave.IsEnabled = false;
                menuSave.IsEnabled = false;
                menuSaveAs.IsEnabled = false;
            }
        }


        private void OnNewLINQ(object sender, RoutedEventArgs e)
        {
            QueryDocument uq = new QueryDocument();
            uq.Title = "New Query";
            uq.Initialize(this.cmbDBPath.Text);
            SetDefaultSettings(uq);
            uq.Show(this.dockingManager1);
            uq.Activate();

            btnExecute.IsEnabled = true;
            menuExecute.IsEnabled = true;
            btnSave.IsEnabled = true;
            menuSave.IsEnabled = true;
            menuSaveAs.IsEnabled = true;
        }
        void newCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.OnNewLINQ(sender, e);
        }
       

        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void menuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            QueryDocument query = dockingManager1.ActiveDocument as QueryDocument;

            if (query != null)
            {
                query.SaveAs();
                string fileName = query.GetFile();
                if (fileName != null)
                {
                    string fname = System.IO.Path.GetFileName(fileName);
                    query.Title = fname;
                }
            }

        }

        private void menuSave_Click(object sender, RoutedEventArgs e)
        {
            QueryDocument query = dockingManager1.ActiveDocument as QueryDocument;

            if (query != null)
            {
                query.Save();
                string fileName = query.GetFile();
                if (fileName != null)
                {
                    string fname = System.IO.Path.GetFileName(fileName);
                    query.Title = fname;
                }
            }
        }
        void saveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.menuSave_Click(sender, e);
        }
        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog opf = new System.Windows.Forms.OpenFileDialog();
            opf.Filter = "(*.linq)|*.linq|All Files(*.*)|*.*";
            opf.InitialDirectory = Environment.CurrentDirectory;
            if (opf.ShowDialog(this.GetIWin32Window()) == System.Windows.Forms.DialogResult.OK)
            {
                using (StreamReader sr = new StreamReader(opf.FileName))
                {
                    string s = sr.ReadToEnd();
                    QueryDocument uq = new QueryDocument();
                    uq.Title = opf.FileName;
                    uq.Initialize(this.cmbDBPath.Text);
                    this.SetDefaultSettings(uq);
                    uq.Show(this.dockingManager1);
                    uq.SetText(s, opf.FileName);
                    uq.Activate();

                    btnExecute.IsEnabled = true;
                    menuExecute.IsEnabled = true;
                    btnSave.IsEnabled = true;
                    menuSave.IsEnabled = true;
                    menuSaveAs.IsEnabled = true;
                }
            }
        }
        void openCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.menuOpen_Click(sender, e);
        }
        private void menuExecute_Click(object sender, RoutedEventArgs e)
        {
            QueryDocument query = dockingManager1.ActiveDocument as QueryDocument;
            DocsDocument queryBucket = dockingManager1.ActiveDocument as DocsDocument;
            if (query != null)
            {
                query.Execute(this.cmbDBPath.Text);
            }
            else if (queryBucket != null)
            {
                queryBucket.Execute();
            }
        }
        void execCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.menuExecute_Click(sender, e);
        }


        private void menuReferences_Click(object sender, RoutedEventArgs e)
        {
            AddReference adref = new AddReference();
            adref.ShowDialog();

        }

      
        private void menuAbout_Click(object sender, RoutedEventArgs e)
        {
            About ab = new About();
            ab.ShowDialog();
        }
        private void menuHelp_Click(object sender, RoutedEventArgs e)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Help\\Help.rtf";
            try
            {
                Process.Start(path);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Cannot open Help.rtf");
            }
        }
        private void btnNewLINQ_Click(object sender, RoutedEventArgs e)
        {
            this.OnNewLINQ(sender, e);
        }

        private void btnOpenLINQ_Click(object sender, RoutedEventArgs e)
        {
            this.menuOpen_Click(sender, e);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.menuSave_Click(sender, e);
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            this.menuExecute_Click(sender, e);
        }
        string prevPath = null;
        private void btnOpenDB_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fb = new System.Windows.Forms.FolderBrowserDialog();
            if (prevPath != null)
            {
                fb.SelectedPath = prevPath;
            }
            else
            {
                fb.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
               
            }
            
            fb.Description = "Select a database folder";
            
            if (fb.ShowDialog(this.GetIWin32Window()) == System.Windows.Forms.DialogResult.OK)
            {
                this.cmbDBPath.Text = fb.SelectedPath;
                prevPath = fb.SelectedPath;
            }
        }
        List<Sqo.MetaType> siaqodbList;
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.Directory.Exists(cmbDBPath.Text))
            {
                if (!cmbDBPath.Items.Contains(cmbDBPath.Text))
                {
                    cmbDBPath.Items.Add(cmbDBPath.Text);
                    SiaqodbConfigurator.EncryptedDatabase = false;
                    using (Sqo.Siaqodb siaqodbConfig = new Sqo.Siaqodb(App.ConfigDbPath))
                    {
                        siaqodbConfig.StoreObject(new ConnectionItem(cmbDBPath.Text));
                    }
                    EncryptionSettings.SetEncryptionSettings();//set back settings
                }
                try
                {
                    siaqodb = Sqo.Internal._bs._b(cmbDBPath.Text);
                }
                catch (Exception ex)
                {
                    if (ex.Message.StartsWith("MDB_INVALID: File is not an LMDB"))
                    {
                        string msg = "";
                        if (IntPtr.Size == 8)//x64
                        {
                            msg = "Database is created on a x86 platform, open it wiht SiaqodbManager x86 version";
                        }
                        else//32 bit
                        {
                            msg = "Database is created on a x64 platform, open it wiht SiaqodbManager x64 version";
                        }
                        System.Windows.Forms.MessageBox.Show(msg);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(ex.Message);
                    }
                    return;
                }
                
                siaqodbList = siaqodb.GetAllTypes();
                treeView1.Items.Clear();
                
                ImageTreeViewItem objectsNode = new ImageTreeViewItem();
                objectsNode.Text = "Objects";
                objectsNode.SelectedImage = ImageTreeViewItem.Createimage(@"pack://application:,,/Resources/objects-icon.png");
                objectsNode.UnselectedImage = ImageTreeViewItem.Createimage(@"pack://application:,,/Resources/objects-icon.png");
                treeView1.Items.Add(objectsNode);

                ImageTreeViewItem documentsNode = new ImageTreeViewItem();
                documentsNode.Text = "Documents";
                documentsNode.SelectedImage = ImageTreeViewItem.Createimage(@"pack://application:,,/Resources/documents.png");
                documentsNode.UnselectedImage = ImageTreeViewItem.Createimage(@"pack://application:,,/Resources/documents.png");
                treeView1.Items.Add(documentsNode);

                ContextMenu cm = new ContextMenu();
                MenuItem mitem = new MenuItem();
                mitem.Header = "Load objects";
                cm.Items.Add(mitem);
                mitem.Click += new RoutedEventHandler(mitem_Click);
                foreach (Sqo.MetaType mt in siaqodbList)
                {
                    
                    //Sqo.Internal._bs._sdbfn(siaqodb, mt, mt.FileName);
                    ImageTreeViewItem nodeType = new ImageTreeViewItem();
                    nodeType.Tag = mt;
                    nodeType.Text = mt.Name;
                    nodeType.SelectedImage = ImageTreeViewItem.Createimage(@"pack://application:,,/Resources/pubclass.gif");
                    nodeType.UnselectedImage = ImageTreeViewItem.Createimage(@"pack://application:,,/Resources/pubclass.gif");
                    nodeType.ContextMenu = cm;

                    objectsNode.Items.Add(nodeType);
                    foreach (Sqo.MetaField mf in mt.Fields)
                    {
                        ImageTreeViewItem nodeField = new ImageTreeViewItem();
                        //nodeField.Header = mf.Name + "(" + mf.FieldType.ToString() + ")";
                        if (mf.FieldType != null)
                        {
                            nodeField.Text = mf.Name + "(" + mf.FieldType.ToString() + ")";
                        }
                        else
                        {
                            nodeField.Text = mf.Name + "(ComplexType)";
                        }
                        nodeField.SelectedImage = ImageTreeViewItem.Createimage(@"pack://application:,,/Resources/pubfield.gif");
                        nodeField.UnselectedImage = ImageTreeViewItem.Createimage(@"pack://application:,,/Resources/pubfield.gif");
                        nodeField.ContextMenu = null;
                        nodeType.Items.Add(nodeField);

                    }

                }
                var buckets = siaqodb.Documents.GetAllBuckets();
                foreach (string buk in buckets)
                {
                    ImageTreeViewItem bucketNode = new ImageTreeViewItem();
                    bucketNode.Text = buk;
                    bucketNode.Tag = buk;
                    bucketNode.SelectedImage = ImageTreeViewItem.Createimage(@"pack://application:,,/Resources/bucket.png");
                    bucketNode.UnselectedImage = ImageTreeViewItem.Createimage(@"pack://application:,,/Resources/bucket.png");
                    documentsNode.Items.Add(bucketNode);

                    btnDBInfo.IsEnabled = true;
                }
            }
            else
            {
                MessageBox.Show("Select a valid folder path!");
            }
        }

        void mitem_Click(object sender, RoutedEventArgs e)
        {
            LoadObjects();
        }

        

        private void treeView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LoadObjects();
        }
        private void LoadObjects()
        {
            TreeViewItem item = treeView1.SelectedItem as TreeViewItem;
            if (item != null)
            {
                MetaType mt = item.Tag as MetaType;
                string bucketName = item.Tag as string;
                if (mt != null)
                {
                    ObjectsDocument uco = new ObjectsDocument();
                    uco.Initialize(mt, siaqodb, siaqodbList);
                    uco.OpenObjects += new EventHandler<MetaEventArgs>(uco_OpenObjects);
                    uco.Title = mt.Name;
                    SetDefaultSettings(uco);
                    uco.Show(this.dockingManager1);
                    uco.Activate();
                    btnExecute.IsEnabled = false;
                    menuExecute.IsEnabled = false;
                    btnSave.IsEnabled = false;
                    menuSave.IsEnabled = false;
                    menuSaveAs.IsEnabled = false;
                }
                else if (bucketName != null)
                {
                    DocsDocument uco = new DocsDocument();
                    uco.Title ="Bucket:"+ bucketName;
                    uco.Initialize(bucketName,siaqodb);
                    SetDefaultSettings(uco);
                    uco.Show(this.dockingManager1);
                    uco.Activate();
                    btnExecute.IsEnabled = true;
                    menuExecute.IsEnabled = true;
                    btnSave.IsEnabled = false;
                    menuSave.IsEnabled = false;
                    menuSaveAs.IsEnabled = false;
                }
                
            }
        }

        void uco_OpenObjects(object sender, MetaEventArgs e)
        {
            ObjectsDocument uco = new ObjectsDocument();
            uco.Initialize(e.mType, siaqodb, siaqodbList,e.oids);
            uco.OpenObjects += new EventHandler<MetaEventArgs>(uco_OpenObjects);
            uco.Title = e.mType.Name;
            SetDefaultSettings(uco);
            uco.Show(this.dockingManager1);
            uco.Activate();
            btnExecute.IsEnabled = false;
            menuExecute.IsEnabled = false;
            btnSave.IsEnabled = false;
            menuSave.IsEnabled = false;
            menuSaveAs.IsEnabled = false;
        }
        private void SetDefaultSettings(DocumentContent doc)
        {
            doc.MaxWidth = 150;
            doc.ToolTip = doc.Title;
            
        }

        private void menuEncryption_Click(object sender, RoutedEventArgs e)
        {
            EncryptionSettings es = new EncryptionSettings();
            bool? res= es.ShowDialog();
            if (res.HasValue )
            {
                if (res.Value)
                {
                    List<ObjectsDocument> list = new List<ObjectsDocument>();
                    foreach (DocumentContent doc in dockingManager1.Documents)
                    {
                        ObjectsDocument d = doc as ObjectsDocument;
                        if (d != null)
                        {
                            list.Add(d);
                        }
                    }
                    foreach (DocumentContent doc in list)
                    {
                        doc.Close();
                    }
                    treeView1.Items.Clear();
                }
            }
        }

        private void btnDBInfo_Click(object sender, RoutedEventArgs e)
        {
            if (siaqodbList != null)
            {
                foreach (Sqo.MetaType mt in siaqodbList)
                {

                }
                DatabaseInfo dbinfo = new DatabaseInfo();

                dbinfo.Title ="Database info";
                SetDefaultSettings(dbinfo);
                dbinfo.Initialize(siaqodb,siaqodbList);
                dbinfo.Show(this.dockingManager1);
                dbinfo.Activate();
            }
        }

    }
}
