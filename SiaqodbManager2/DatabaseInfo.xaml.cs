using AvalonDock;
using SiaqodbManager.Entities;
using Sqo;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace SiaqodbManager
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class DatabaseInfo : DocumentContent
    {
        public DatabaseInfo()
        {
            InitializeComponent();
        }
        System.Windows.Forms.DataGridView dataGridView1 = null;
        List<Sqo.MetaType> typesList;
        Siaqodb siaqodb;
        private void DatabaseInfo_Loaded(object sender, RoutedEventArgs e)
        {

        }
        public void Initialize(Siaqodb siaqodb, List<Sqo.MetaType> typesList)
        {
            this.SetGridDesigner();
            

            this.typesList = typesList;
            this.siaqodb = siaqodb;
            dbPath.Content = siaqodb.GetDBPath();
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(siaqodb.GetDBPath());
            if (di.Exists)
            {
                dbSize.Content ="Total: "+ ((decimal)(((decimal)DirSize(di))/(1024*1024))).ToString("0.00")+ " MB";
            }
            dbSize.Content+="; Used: "+((decimal)(decimal)siaqodb.DbInfo.UsedSize/(1024*1024)).ToString("0.00") + " MB";
            dbSize.Content += "; Free: " + ((decimal)(decimal)siaqodb.DbInfo.FreeSpace / (1024 * 1024)).ToString("0.00") + " MB";

            nrTypes.Content = typesList.Count.ToString();
            lblTitle.Content = "Database: "+di.Name ;
           
           
            dataGridView1.RowCount = typesList.Count;
        }

        void dataGridView1_CellValueNeeded(object sender, System.Windows.Forms.DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex > typesList.Count - 1)
            {

            }
            else
            {
                if (e.ColumnIndex == 0)
                {
                    e.Value = typesList[e.RowIndex].Name;
                }
               
                else if (e.ColumnIndex == 1)
                {
                    e.Value = siaqodb.LoadAllOIDs(typesList[e.RowIndex]).Count;
                }
                else if (e.ColumnIndex == 2)
                {
                    MetaType mt = typesList[e.RowIndex];
                    e.Value = mt.Fields.Count;
                }
               
            }
        }
        private void SetGridDesigner()
        {

            dataGridView1 = new System.Windows.Forms.DataGridView();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();

            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ReadOnly = true;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(648, 516);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.VirtualMode = true;
            this.dataGridView1.CellValueNeeded += dataGridView1_CellValueNeeded;

            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add("TypeName", "TypeName");
            dataGridView1.Columns.Add("NrTotalObjects", "Total Objects");
            dataGridView1.Columns.Add("NumberOfFields", "Number of Fields");
            

            myhost.Child = dataGridView1;

        }
        public static long DirSize(DirectoryInfo d)
        {
            long Size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                Size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                Size += DirSize(di);
            }
            return (Size);
        }

    }
}
