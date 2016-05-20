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
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;

namespace SiaqodbManager
{
    /// <summary>
    /// Interaction logic for AddReference.xaml
    /// </summary>
    public partial class AddReference : Window
    {
        public AddReference()
        {
            InitializeComponent();
        }
        private List<ReferenceItem> assemblies = new List<ReferenceItem>();
        private List<NamespaceItem> namespaces = new List<NamespaceItem>();
        string prevPath = null;
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "assembly files (*.dll;*.exe)|*.dll;*.exe";
            if (prevPath == null)
            {
                opf.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                opf.InitialDirectory = prevPath;
            }
            opf.Multiselect = true;
            if (opf.ShowDialog(this.GetIWin32Window()) == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string fileName in opf.FileNames)
                {
                    listBox1.Items.Add(fileName);
                    prevPath = System.IO.Path.GetDirectoryName(fileName);
                }
               
               
            }
        }
        

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (this.listBox1.SelectedItem != null)
            {
                this.listBox1.Items.Remove(this.listBox1.SelectedItem);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(App.ConfigDbPath))
            {
                assemblies.Clear();
                namespaces.Clear();
                Sqo.SiaqodbConfigurator.EncryptedDatabase = false;

                Sqo.Siaqodb siaqodb = new Sqo.Siaqodb(App.ConfigDbPath);
                try
                {
                    siaqodb.DropType<ReferenceItem>();
                    siaqodb.DropType<NamespaceItem>();
                    foreach (object o in listBox1.Items)
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
                    foreach (string s in textBox1.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        NamespaceItem nobj = new NamespaceItem(s);
                        namespaces.Add(nobj);
                        siaqodb.StoreObject(nobj);
                    }
                }
                finally
                {
                    siaqodb.Close();
                    EncryptionSettings.SetEncryptionSettings();//set back settings
                }
            }

            this.DialogResult = true;
        }
        public List<ReferenceItem> GetReferences()
        {
            return assemblies;
        }
        public List<NamespaceItem> GetNamespaces()
        {
            return namespaces;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(App.ConfigDbPath))
            {
                Sqo.SiaqodbConfigurator.EncryptedDatabase = false;
                Sqo.Siaqodb siaqodb = new Sqo.Siaqodb(App.ConfigDbPath);
                try
                {
                    Sqo.IObjectList<ReferenceItem> references = siaqodb.LoadAll<ReferenceItem>();
                    foreach (ReferenceItem refItem in references)
                    {
                        listBox1.Items.Add(refItem);
                    }
                    Sqo.IObjectList<NamespaceItem> namespacesItems = siaqodb.LoadAll<NamespaceItem>();
                    foreach (NamespaceItem nItem in namespacesItems)
                    {
                        textBox1.Text += nItem + Environment.NewLine;
                    }
                }
                finally
                {
                    siaqodb.Close();
                    EncryptionSettings.SetEncryptionSettings();//set back settings
                }

            }
        }

        private void btnAddDefault_Click(object sender, RoutedEventArgs e)
        {
            listBox1.Items.Add(typeof(object).Assembly.Location);
            listBox1.Items.Add(typeof(RuntimeBinderException).Assembly.Location);
            listBox1.Items.Add(typeof(System.Runtime.CompilerServices.DynamicAttribute).Assembly.Location);
            listBox1.Items.Add(typeof(Sqo.Siaqodb).Assembly.Location);

        }
    }
}
