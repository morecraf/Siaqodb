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
using System.IO;
using AvalonDock;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;

namespace SiaqodbManager
{
    /// <summary>
    /// Interaction logic for Document.xaml
    /// </summary>
    public partial class QueryDocument : DocumentContent
    {
        public QueryDocument()
        {
            InitializeComponent();

            DataContext = this;
        }

        #region TextContent

        /// <summary>
        /// TextContent Dependency Property
        /// </summary>
        public static readonly DependencyProperty TextContentProperty =
            DependencyProperty.Register("TextContent", typeof(string), typeof(QueryDocument),
                new FrameworkPropertyMetadata((string)string.Empty,
                    new PropertyChangedCallback(OnTextContentChanged)));

        /// <summary>
        /// Gets or sets the TextContent property.  This dependency property 
        /// indicates document text.
        /// </summary>
        public string TextContent
        {
            get { return (string)GetValue(TextContentProperty); }
            set { SetValue(TextContentProperty, value); }
        }

        /// <summary>
        /// Handles changes to the TextContent property.
        /// </summary>
        private static void OnTextContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((QueryDocument)d).OnTextContentChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the TextContent property.
        /// </summary>
        protected virtual void OnTextContentChanged(DependencyPropertyChangedEventArgs e)
        {
            if (TextContentChanged != null)
                TextContentChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// event raised when text changes
        /// </summary>
        public event EventHandler TextContentChanged;
        #endregion

        string path;
        System.Windows.Forms.DataGridView dataGridView1;
        public void Initialize(string path)
        {
            //string appPath = System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            textEditor1.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");

            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView1.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(3, 3);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(536, 209);
            this.dataGridView1.TabIndex = 0;

            this.gridHost.Child = this.dataGridView1;
            
            
        }
        private string file;
        public void Save()
        {
            if (this.file == null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.DefaultExt = ".linq";
                sfd.Filter = "(*.linq)|*.linq|All Files(*.*)|*.*";
                DialogResult dg = sfd.ShowDialog(this.GetIWin32Window());
                if (dg == DialogResult.OK)
                {

                    using (StreamWriter sw = new StreamWriter(sfd.FileName))
                    {
                        sw.Write(this.textEditor1.Text);
                        this.file = sfd.FileName;
                    }
                }
            }
            else
            {
                using (StreamWriter sw = new StreamWriter(this.file))
                {
                    sw.Write(this.textEditor1.Text);
                }
            }
        }
        public void SaveAs()
        {

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".linq";
            sfd.Filter = "(*.linq)|*.linq|All Files(*.*)|*.*";
            DialogResult dg = sfd.ShowDialog(this.GetIWin32Window());
            if (dg == DialogResult.OK)
            {

                using (StreamWriter sw = new StreamWriter(sfd.FileName))
                {
                    sw.Write(this.textEditor1.Text);
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
            
            Sqo.Siaqodb siaqodbConfig = new Sqo.Siaqodb(AppDomain.CurrentDomain.BaseDirectory + "\\config");
            Sqo.IObjectList<NamespaceItem> namespaces = siaqodbConfig.LoadAll<NamespaceItem>();
            Sqo.IObjectList<ReferenceItem> references = siaqodbConfig.LoadAll<ReferenceItem>();
            siaqodbConfig.Close();
            
            EncryptionSettings.SetEncryptionSettings();//set back settings

            string ifEncrypted = "";
            if (EncryptionSettings.IsEncryptedChecked)
            {
                ifEncrypted = @" SiaqodbConfigurator.EncryptedDatabase=true;
                                 SiaqodbConfigurator.SetEncryptor(BuildInAlgorithm."+EncryptionSettings.Algorithm+@"); 

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
			
							object list= (" + this.textEditor1.Text + @").ToList();
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

        internal void SetText(string s, string file)
        {
            this.textEditor1.Text = s;
            this.file = file;
        }





    }
}
