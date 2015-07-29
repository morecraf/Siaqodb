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
using SiaqodbManager.ViewModel;
using SiaqodbManager.Entities;
using SiaqodbManager.DialogService;
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
        private MainViewModel viewModel;
        public MainWindow()
        {
            viewModel = new MainViewModel();
            DataContext = viewModel;
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
            dockingManager1.ActiveDocumentChanged += new EventHandler(dockingManager1_ActiveDocumentChanged);
            dockingManager1.DocumentClosed += new EventHandler(dockingManager1_DocumentClosed);
            dockingManager1.DocumentClosing += new EventHandler<System.ComponentModel.CancelEventArgs>(dockingManager1_DocumentClosing);
            //btnExecute.IsEnabled = false;
            //menuExecute.IsEnabled = false;
            //btnSave.IsEnabled = false;
            //menuSave.IsEnabled = false;
            //menuSaveAs.IsEnabled = false;
            //btnDBInfo.IsEnabled = false;
        }

        void dockingManager1_DocumentClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (prev != null)
            {
                if (prev.IsLoaded)
                {
                    prev.Activate();
                }
                else
                {
                   
                }
            }
        }

        void dockingManager1_DocumentClosed(object sender, EventArgs e)
        {
            
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
            var queryViewModel = viewModel.CreateQueryModel(new SaveLinqDialogService());
            QueryDocument uq = new QueryDocument(queryViewModel);
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
                    var queryViewModel = viewModel.CreateQueryModel(new SaveLinqDialogService());
                    QueryDocument uq = new QueryDocument(queryViewModel);
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
            if (query != null)
            {
                query.Execute(this.cmbDBPath.Text);
            }
        }
        void execCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.menuExecute_Click(sender, e);
        }

        private void menuReferences_Click(object sender, RoutedEventArgs e)
        {
            AddReference adref = new AddReference();
            if (adref.ShowDialog() == true)
            {

            }
        }

        private void ChangeColor(object sender, RoutedEventArgs e)
        {
            Color c = (Color)ColorConverter.ConvertFromString(((MenuItem)sender).Header.ToString());
            ThemeFactory.ChangeColors(c);
            //this.treeView1.Background = new LinearGradientBrush(c, Color.FromRgb(255, 255, 255),45);
        }
        private void ChangeStandardTheme(object sender, RoutedEventArgs e)
        {
            string name = (string)((MenuItem)sender).Tag;
            ThemeFactory.ChangeTheme(name);
        }
        private void SetDefaultTheme(object sender, RoutedEventArgs e)
        {
            ThemeFactory.ResetTheme();
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

        private void btnOpenDB_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fb = new System.Windows.Forms.FolderBrowserDialog();
            
            fb.Description = "Select a database folder";
            
            if (fb.ShowDialog(this.GetIWin32Window()) == System.Windows.Forms.DialogResult.OK)
            {
                this.cmbDBPath.Text = fb.SelectedPath;
            }
        }
        List<Sqo.MetaType> siaqodbList;


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
            var metaType = treeView1.SelectedItem as MetaTypeViewModel;
            if(metaType != null){
                var objectsModel = viewModel.CreateObjectsModel(metaType);
                objectsModel.OpenObjects += new EventHandler<MetaEventArgs>(uco_OpenObjects);
                ObjectsDocument uco = new ObjectsDocument(objectsModel);
                uco.Initialize(metaType.MetaType, viewModel.Siaqodb, siaqodbList);
                uco.Title = metaType.Name;
                SetDefaultSettings(uco);
                uco.Show(this.dockingManager1);
                uco.Activate();
            //    viewModel.OnObjectLoad(metaType);
            //    uco.DataContext = viewModel.ObjectsTable;
            }
            //if (item != null)
            //{
            //    MetaType mt = item.Tag as MetaType;
            //    if (mt != null)
            //    {
            //        ObjectsDocument uco = new ObjectsDocument();
            //        uco.Initialize(mt, siaqodb,siaqodbList);
            //        uco.Title = mt.Name;
            //        SetDefaultSettings(uco);
            //        uco.Show(this.dockingManager1);
            //        uco.Activate();
            //        btnExecute.IsEnabled = false;
            //        menuExecute.IsEnabled = false;
            //        btnSave.IsEnabled = false;
            //        menuSave.IsEnabled = false;
            //        menuSaveAs.IsEnabled = false;
            //    }
            //}
        }

        void uco_OpenObjects(object sender, MetaEventArgs e)
        {
            var objectsModel = viewModel.CreateObjectsModel(new MetaTypeViewModel(e.mType),e.oids);
            objectsModel.OpenObjects += new EventHandler<MetaEventArgs>(uco_OpenObjects);
            ObjectsDocument uco = new ObjectsDocument(objectsModel);
            uco.Initialize(e.mType, siaqodb, siaqodbList);
            uco.Title = e.mType.Name;
            SetDefaultSettings(uco);
            uco.Show(this.dockingManager1);
            uco.Activate();
            //btnExecute.IsEnabled = false;
            //menuExecute.IsEnabled = false;
            //btnSave.IsEnabled = false; 
            //menuSave.IsEnabled = false;
            //menuSaveAs.IsEnabled = false;
        }
        private void SetDefaultSettings(DocumentContent doc)
        {
            doc.MaxWidth = 150;
            doc.ToolTip = doc.Title;
            
        }

        private void menuEncryption_Click(object sender, RoutedEventArgs e)
        {
            EncryptionSettings es = new EncryptionSettings(EncryptionViewModel.Instance);
            bool? res = es.ShowDialog();
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
