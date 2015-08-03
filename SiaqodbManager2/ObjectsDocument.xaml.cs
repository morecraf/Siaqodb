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

using AvalonDock;
using Sqo;
using Sqo.Exceptions;
using System.Collections;
using Sqo.Internal;
using SiaqodbManager.ViewModel;
using SiaqodbManager.Entities;
using SiaqodbManager.Repo;

namespace SiaqodbManager
{
    /// <summary>
    /// Interaction logic for Document.xaml
    /// </summary>
    public partial class ObjectsDocument : DocumentContent
    {
        private ObjectViewModel viewModel;
        public ObjectsDocument(ObjectViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
        }

        #region TextContent

        /// <summary>
        /// TextContent Dependency Property
        /// </summary>
        public static readonly DependencyProperty TextContentProperty =
            DependencyProperty.Register("TextContent", typeof(string), typeof(ObjectsDocument),
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
            ((ObjectsDocument)d).OnTextContentChanged(e);
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

        List<int> oids = null;
        MetaType metaType;
        System.Windows.Forms.DataGridView dataGridView1 = null;
        List<Sqo.MetaType> typesList;
        internal event EventHandler<MetaEventArgs> OpenObjects;
        Dictionary<int, MetaType> columnsTypes = new Dictionary<int, MetaType>();

        internal void Initialize(MetaType metaType,  List<Sqo.MetaType> typesList)
        {
            dataGridView1 = new System.Windows.Forms.DataGridView();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();

            this.dataGridView1.AllowUserToAddRows = true;
            this.dataGridView1.AllowUserToDeleteRows = true;
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
            
            this.dataGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellValueChanged);
            this.dataGridView1.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.dataGridView1_CellValueNeeded);
            this.dataGridView1.CellValuePushed += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.dataGridView1_CellValuePushed);
            this.dataGridView1.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(dataGridView1_UserDeletingRow);
            this.dataGridView1.UserDeletedRow += new System.Windows.Forms.DataGridViewRowEventHandler(dataGridView1_UserDeletedRow);
            this.dataGridView1.UserAddedRow += new System.Windows.Forms.DataGridViewRowEventHandler(dataGridView1_UserAddedRow);
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(dataGridView1_CellContentClick);
            this.dataGridView1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(dataGridView1_CellDoubleClick);
            this.dataGridView1.ColumnHeaderMouseClick += dataGridView1_ColumnHeaderMouseClick;
            myhost.Child = dataGridView1;
            this.typesList = typesList;
            this.metaType = metaType;
         
            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add("OID", "OID");
            //dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.SystemColors.ControlDark;
            foreach (MetaField f in metaType.Fields)
            {
                if (typeof(IList).IsAssignableFrom(f.FieldType))
                {
                    System.Windows.Forms.DataGridViewLinkColumn column = new System.Windows.Forms.DataGridViewLinkColumn();
                    column.Name = f.Name;
                    column.HeaderText = f.Name;
                    column.ValueType = f.FieldType;
                    dataGridView1.Columns.Add(column);
                }
                else if (f.FieldType == null)//complex type
                {
                    System.Windows.Forms.DataGridViewLinkColumn column = new System.Windows.Forms.DataGridViewLinkColumn();
                    column.Name = f.Name;
                    column.HeaderText = f.Name;
                    column.ValueType = typeof(string);
                    dataGridView1.Columns.Add(column);
                }
                else
                {
                    dataGridView1.Columns.Add(f.Name, f.Name);
                }
            }
            dataGridView1.RowCount = viewModel.NrOFObjects + 1;
        }
        int currentSortedColumn = -1;
        System.Windows.Forms.SortOrder currentSortOrder;
        void dataGridView1_ColumnHeaderMouseClick(object sender, System.Windows.Forms.DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex > 0)
            {
                MetaField fi = metaType.Fields[e.ColumnIndex - 1];
                if (fi.FieldType == null || typeof(IList).IsAssignableFrom(fi.FieldType))//complex type
                {
                    return;
                }
            }
            if (currentSortedColumn != -1)
            {
                dataGridView1.Columns[currentSortedColumn].HeaderCell.SortGlyphDirection = System.Windows.Forms.SortOrder.None;
                dataGridView1.Columns[currentSortedColumn].HeaderCell.Style.BackColor = System.Drawing.SystemColors.Control;
            }
            if (e.ColumnIndex == currentSortedColumn)
            {
                if (currentSortOrder == System.Windows.Forms.SortOrder.Ascending)
                {
                    dataGridView1.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = System.Windows.Forms.SortOrder.Descending;
                    currentSortOrder = System.Windows.Forms.SortOrder.Descending;
                }
                else
                {
                    dataGridView1.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = System.Windows.Forms.SortOrder.Ascending;
                    currentSortOrder = System.Windows.Forms.SortOrder.Ascending;
                }
            }
            else
            {
                dataGridView1.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = System.Windows.Forms.SortOrder.Ascending;
                currentSortOrder = System.Windows.Forms.SortOrder.Ascending;
            }
            dataGridView1.Columns[e.ColumnIndex].HeaderCell.Style.BackColor = System.Drawing.SystemColors.ControlDark;
            currentSortedColumn = e.ColumnIndex;
            this.SortByColumn();
        }

        private void SortByColumn()
        {
            if (currentSortedColumn == 0)
            {
                if (currentSortOrder == System.Windows.Forms.SortOrder.Ascending)
                {
                    oids = oids.OrderBy(a => a).Select(a => a).ToList();
                }
                else
                {
                    oids = oids.OrderByDescending(a => a).Select(a => a).ToList();
                }
               
            }
            else
            {
                List<SortableEntity> seList = new List<SortableEntity>();
                foreach (int oid in oids)
                {
                    object val = SiaqodbRepo.Instance.LoadValue(oid, metaType.Fields[currentSortedColumn - 1].Name, metaType);
                    SortableEntity se = new SortableEntity() { OID = oid, SortableValue = val };
                    seList.Add(se);
                }
                if (currentSortOrder == System.Windows.Forms.SortOrder.Ascending)
                {
                    oids = seList.OrderBy(a => a.SortableValue).Select(a => a.OID).ToList();
                }
                else
                {
                    oids = seList.OrderByDescending(a => a.SortableValue).Select(a => a.OID).ToList();
                }
            }
            dataGridView1.Refresh();
        }
  
        void dataGridView1_CellDoubleClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > 0 )
            {
                if (this.dataGridView1.Columns[e.ColumnIndex] is System.Windows.Forms.DataGridViewLinkColumn)
                {
                    if (this.dataGridView1.Columns[e.ColumnIndex].ValueType == typeof(string))
                    {
                        var columnName = dataGridView1.Columns[e.ColumnIndex].HeaderText;
                        viewModel.EditComplexObject(e.RowIndex, e.ColumnIndex,columnName);
                    }
                    else
                    {
                        this.EditArray(e.RowIndex, e.ColumnIndex);
                    }
                }
            }
        }

        void dataGridView1_CellContentClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            //click on array
            if (e.RowIndex >= 0)
            {
                if (this.dataGridView1.Columns[e.ColumnIndex] is System.Windows.Forms.DataGridViewLinkColumn)
                {
                    if (this.dataGridView1.Columns[e.ColumnIndex].ValueType == typeof(string))
                    {
                        var columnName = dataGridView1.Columns[e.ColumnIndex].HeaderText;
                        viewModel.EditComplexObject(e.RowIndex,e.ColumnIndex, columnName);
                    }
                    else
                    {
                        this.EditArray(e.RowIndex, e.ColumnIndex);
                    }
                }
            }
        }

        private MetaType FindMeta(int TID)
        {
            return typesList.First<MetaType>(tii => tii.TypeID == TID);
        }
        private void EditArray(int rowIndex,int columnIndex)
        {
            object val = this.dataGridView1.Rows[rowIndex].Cells[columnIndex].Value;
            
            EditArrayWindow eaw = new EditArrayWindow();
            eaw.SetArrayType(this.dataGridView1.Columns[columnIndex].ValueType);
            if (this.dataGridView1.Columns[columnIndex].ValueType == typeof(byte[]))
            {
                MessageBox.Show("Binary data cannot be edited!");
                return;
            }
            if (val != null && val is Array)
            {

                Array ar = (Array)val;
                eaw.SetArrayValue(ar);
            }
            bool? dialog=eaw.ShowDialog();
            if (dialog.HasValue && dialog.Value)
            {
                Array ar = eaw.GetArrayValues();

                try
                {
                    viewModel.EditArray(ar,rowIndex,columnIndex);
                    dataGridView1.Rows[rowIndex].Cells[columnIndex].ErrorText = string.Empty;
                }
                catch (SiaqodbException ex)
                {
                    if (ex.Message.StartsWith("Type of value should be:"))
                    {
                        dataGridView1.Rows[rowIndex].Cells[columnIndex].ErrorText = ex.Message;
                    }
                }
                catch (Exception ex)
                {
                    dataGridView1.Rows[rowIndex].Cells[columnIndex].ErrorText = ex.Message;
                }
            }
        }
        void dataGridView1_UserAddedRow(object sender, System.Windows.Forms.DataGridViewRowEventArgs e)
        {
            viewModel.AddRow();
        }

        void dataGridView1_UserDeletedRow(object sender, System.Windows.Forms.DataGridViewRowEventArgs e)
        {
           
        }

        void dataGridView1_UserDeletingRow(object sender, System.Windows.Forms.DataGridViewRowCancelEventArgs e)
        {
            if (e.Row.Cells[0].Value is int)
            {
                if (MessageBox.Show("Are you sure you want to delete this object?", "", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    var index = e.Row.Index;
                    viewModel.RemoveRow(index);
                }
            }
            else//is new
            {
               // int oid = oids[oids.Count-1];
               // Sqo.Internal._bs._do(siaqodb, oid, metaType);
               // oids.Remove(oid);
            }
        }
        private void dataGridView1_CellValueNeeded(object sender, System.Windows.Forms.DataGridViewCellValueEventArgs e)
        {
            if (dataGridView1.Rows[e.RowIndex].IsNewRow)//new record
            {
               
            }
            else
            {
                if (metaType.Fields.Count >= e.ColumnIndex)
                {
                    MetaField fi;
                    if(e.ColumnIndex == 0){
                        fi = new MetaField
                        {
                            Name = "OID",
                            FieldType = typeof(Int32)
                        };
                    }
                    else
                    {
                        fi = metaType.Fields[e.ColumnIndex - 1];
                    }
                    e.Value = viewModel.GetValue(fi.Name,e.RowIndex);
                }
            }
        }

        private void UCObjects_Load(object sender, EventArgs e)
        {
            dataGridView1.VirtualMode = true;
            if (oids != null)
            {
                dataGridView1.RowCount = oids.Count;
            }
            if (oids != null)
            {
                //this.lblNrRows.Text = oids.Count + " rows";
            }
        }

        private void dataGridView1_CellValueChanged(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
        }

        private void dataGridView1_CellValuePushed(object sender, System.Windows.Forms.DataGridViewCellValueEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                return;
            }
            try
            {
                viewModel.UpdateValue(metaType.Fields[e.ColumnIndex - 1].Name,e.RowIndex,e.Value);
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = string.Empty;
            }
            catch (SiaqodbException ex)
            {
                if (ex.Message.StartsWith("Type of value should be:"))
                {
                    dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = ex.Message;
                }
            }
            catch (Exception ex)
            {
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = ex.Message;
            }
        }
    
        private void DocumentContent_Loaded(object sender, RoutedEventArgs e)
        {
            
        }




    }

    public class SortableEntity
    {
        public int OID { get; set; }
        public object SortableValue { get; set; }
    }
    
}
