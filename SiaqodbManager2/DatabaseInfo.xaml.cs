using AvalonDock;
using SiaqodbManager.Entities;
using Sqo;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
            this.FillTables();
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

        List<TablePK> tablesList = new List<TablePK>();
        private void FillTables()
        {

            string sql = @"SELECT tbls.name as Table_Name,ccu.COLUMN_NAME
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
        inner JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu ON tc.CONSTRAINT_NAME = ccu.Constraint_name
		inner join sys.tables tbls on tbls.name=ccu.TABLE_NAME 
    WHERE tc.CONSTRAINT_TYPE = 'Primary Key'  and tbls.[type]='u' and tbls.name not like '%tracking'
	and  exists(select  * from sys.tables tbls2 where tbls2.name like tbls.name+'_tracking')";

            try
            {
                SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SyncServer"].ConnectionString);
                connection.Open();
                SqlCommand cmd = new SqlCommand(sql);
                cmd.Connection = connection;
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    TablePK tp = tablesList.FirstOrDefault(a => a.TableName == reader[0].ToString());
                    if (tp == null)
                    {
                        tp = new TablePK();
                        tp.ColumnNames = new List<string>();
                        tp.TableName = reader[0].ToString();
                        lstTables.Items.Add(tp);
                        tablesList.Add(tp);
                    }

                    tp.ColumnNames.Add(reader[1].ToString());


                }
                connection.Close();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Unable to access your SQL Database server; please ensure the SyncServer connection string is set in your app.config file.\n" + ex.Message);
            }
            lstTables.Items.SortDescriptions.Add(
            new System.ComponentModel.SortDescription("",
            System.ComponentModel.ListSortDirection.Ascending));

            lstTables.SelectAll();
        }

        private void btnChkDuplicates_Click(object sender, RoutedEventArgs e)
        {


            string fileName = System.IO.Path.GetTempFileName() + ".txt";
            bool exists = false;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName))
            {

                foreach (TablePK tk in lstTables.SelectedItems)
                {
                    MetaType mType = typesList.FirstOrDefault(a => a.Name.Contains("." + tk.TableName, StringComparison.OrdinalIgnoreCase));
                    if (mType != null)
                    {
                        List<int> oids = siaqodb.LoadAllOIDs(mType);
                        int j = 0;
                        Dictionary<int, List<object>> oidsAndValues = new Dictionary<int, List<object>>();

                        foreach (string pkColumn in tk.ColumnNames)
                        {
                            MetaField mf = mType.Fields.FirstOrDefault(a => string.Compare(a.Name, "_" + pkColumn, true) == 0);
                            if (mf != null)
                            {

                                foreach (int oid in oids)
                                {
                                    if (j == 0)
                                        oidsAndValues[oid] = new List<object>();

                                    object val = siaqodb.LoadValue(oid, mf.Name, mType);
                                    oidsAndValues[oid].Add(val);
                                }

                            }
                            j++;
                        }
                        int i = 0;
                        foreach (int oidKey in oidsAndValues.Keys)
                        {
                            foreach (int oidKeyInner in oidsAndValues.Keys)
                            {
                                if (oidKeyInner != oidKey)
                                {
                                    bool allEqual = true;
                                    for (int u = 0; u < oidsAndValues[oidKey].Count; u++)
                                    {
                                        if (((IComparable)oidsAndValues[oidKey][u]).CompareTo((IComparable)oidsAndValues[oidKeyInner][u]) != 0)
                                        {
                                            allEqual = false;
                                            break;
                                        }
                                    }
                                    if (allEqual)
                                    {
                                        exists = true;
                                        if (i == 0)
                                        {
                                            file.WriteLine("Duplicates for type:" + mType.Name + ":");
                                        }
                                        StringBuilder sb = new StringBuilder();
                                        foreach (object objDuplicate in oidsAndValues[oidKeyInner])
                                        {
                                            sb.Append(objDuplicate.ToString() + ";");
                                        }
                                        file.WriteLine("OID=(" + oidKey + "," + oidKeyInner + ") | Values=" + sb.ToString());
                                        i++;
                                    }
                                }
                            }

                        }

                        if (i > 0)
                        {
                            file.WriteLine("============================================================");
                            file.WriteLine("");
                        }
                    }
                }
            }
            if (exists)
            {
                System.Diagnostics.Process.Start(fileName);
            }
            else
            {

                System.Windows.Forms.MessageBox.Show("No duplicates found!");
            }

        }

        private class TablePK : IComparable
        {
            public string TableName { get; set; }
            public List<string> ColumnNames { get; set; }
            public override string ToString()
            {
                return TableName;
            }


            public int CompareTo(object obj)
            {
                return this.TableName.CompareTo(((TablePK)obj).TableName);
            }
        }

        private void chkSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            lstTables.SelectAll();


        }
        private void chkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            lstTables.UnselectAll();
        }
    }
}
