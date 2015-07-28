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
using System.Runtime.Serialization.Formatters.Binary;
using SiaqodbManager.ViewModel;

namespace SiaqodbManager
{
    /// <summary>
    /// Interaction logic for Document.xaml
    /// </summary>
    public partial class QueryDocument : DocumentContent
    {
        public QueryDocument(QueryViewModel queryViewModel)
        {
            InitializeComponent();
            DataContext = queryViewModel;
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
        private ViewModel.QueryViewModel queryViewModel;
        public void Save()
        {
            queryViewModel.Save(path);
        }
        public void SaveAs()
        {
            queryViewModel.SaveAs();
        }
        public void Execute(string path)
        {
            queryViewModel.Execute(path);
           
        }
      
        public string GetFile()
        {
            return file;
        }

        internal void SetText(string s, string file)
        {
     //       this.textEditor1.Text = s;
            this.file = file;
        }
    }
}
