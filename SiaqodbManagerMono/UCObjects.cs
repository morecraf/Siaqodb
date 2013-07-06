using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sqo;
using Sqo.Exceptions;
using System.Collections;
using Sqo.Internal;


namespace SiaqodbManager
{
	public partial class UCObjects : UserControl
	{
		public UCObjects()
		{
			InitializeComponent();
		}
		List<int> oids = null;
		Siaqodb siaqodb;
		MetaType metaType;
        List<Sqo.MetaType> typesList;
        internal event EventHandler<MetaEventArgs> OpenObjects;
        Dictionary<int, MetaType> columnsTypes = new Dictionary<int, MetaType>();
        internal void Initialize(MetaType metaType, Siaqodb siaqodb, List<Sqo.MetaType> typesList)
        {
            Initialize(metaType, siaqodb, typesList, null);
        }

        internal void Initialize(MetaType metaType, Siaqodb siaqodb, List<Sqo.MetaType> typesList, List<int> oidsFiltered)
		{
			this.metaType = metaType;
			this.siaqodb = siaqodb;
            this.typesList = typesList;

            if (oidsFiltered == null)
            {
                oids = siaqodb.LoadAllOIDs(metaType);
            }
            else
            {
                oids = oidsFiltered;
            }
            if (oids == null)
            {
                MessageBox.Show("FileName of this Type has not default name of siaqodb database file!");
            }
			dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add("OID", "OID");
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
			
		}
        private void UCObjects_Load(object sender, EventArgs e)
        {
            dataGridView1.VirtualMode = true;
            if (oids != null)
            {
                dataGridView1.RowCount = oids.Count+1;
            }
            if (oids != null)
            {
                this.lblNrRows.Text = oids.Count + " rows";
            }
        }
        protected void OnOpenObjects(MetaEventArgs args)
        {
            if (this.OpenObjects != null)
            {
                this.OpenObjects(this, args);
            }
        }
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            
            
            if (e.RowIndex > 0)
            {
                if (this.dataGridView1.Columns[e.ColumnIndex] is System.Windows.Forms.DataGridViewLinkColumn)
                {
                    if (this.dataGridView1.Columns[e.ColumnIndex].ValueType == typeof(string))
                    {
                        this.EditComplexObject(e.RowIndex, e.ColumnIndex);
                    }
                    else
                    {
                        this.EditArray(e.RowIndex, e.ColumnIndex);
                    }
                }
            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //click on array
            if (e.RowIndex >= 0)
            {
                if (this.dataGridView1.Columns[e.ColumnIndex] is System.Windows.Forms.DataGridViewLinkColumn)
                {
                    if (this.dataGridView1.Columns[e.ColumnIndex].ValueType == typeof(string))
                    {
                        this.EditComplexObject(e.RowIndex, e.ColumnIndex);
                    }
                    else
                    {
                        this.EditArray(e.RowIndex, e.ColumnIndex);
                    }
                }
            }
        }
        private void EditComplexObject(int rowIndex, int columnIndex)
        {
            MetaField fi = metaType.Fields[columnIndex - 1];

            List<int> oids = new List<int>();
            int TID = 0;
            if (this.dataGridView1.Rows[rowIndex].Cells[0].Value != null)//is not new row
            {
                _bs._loidtid(this.siaqodb, (int)this.dataGridView1.Rows[rowIndex].Cells[0].Value, this.metaType, fi.Name, ref oids, ref TID);
                if (oids.Count == 0 || TID <= 0)
                {

                }
                else
                {

                    MetaType mtOfComplex = FindMeta(TID);

                    this.OnOpenObjects(new MetaEventArgs(mtOfComplex, oids));
                }
            }
        }
        private void EditArray(int rowIndex, int columnIndex)
        {
            return;
            //generate problems on mac osx
            /*
            object val = this.dataGridView1.Rows[rowIndex].Cells[columnIndex].Value;

            EditArray eaw = new EditArray();
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
            DialogResult dialog = eaw.ShowDialog();
            if (dialog==DialogResult.OK)
            {
                Array ar = eaw.GetArrayValues();

                try
                {

                    Sqo.Internal._bs._uf(siaqodb, oids[rowIndex], metaType, metaType.Fields[columnIndex - 1].Name, ar);
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

            }*/
        }
        private MetaType FindMeta(int TID)
        {
            return typesList.First<MetaType>(tii => tii.TypeID == TID);
        }
        private void dataGridView1_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            return;
            //on MacOSX this is called without reason and new objects are created
            /*
            int oid = Sqo.Internal._bs._io(siaqodb, metaType);
            this.oids.Add(oid);*/

        }

        private void dataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (e.Row.Cells[0].Value is int)
            {
                if (MessageBox.Show("Are you sure to delete this object?", "", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    e.Cancel = true;
                }
                else
                {

                    int oid = (int)e.Row.Cells[0].Value;
                    Sqo.Internal._bs._do(siaqodb, oid, metaType);
                    oids.Remove(oid);

                }
            }
            else//is new
            {
                int oid = oids[oids.Count - 1];
                Sqo.Internal._bs._do(siaqodb, oid, metaType);
                oids.Remove(oid);
            }
        }

		private void dataGridView1_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
            if (dataGridView1.Rows[e.RowIndex].IsNewRow)//new record
            {

            }
            else
            {
                if (e.RowIndex > oids.Count - 1)
                {

                }
                else
                {
                    if (e.ColumnIndex == 0)
                    {
                        e.Value = oids[e.RowIndex];
                    }
                    else
                    {
                        MetaField fi = metaType.Fields[e.ColumnIndex - 1];
                        if (fi.FieldType == null)//complex type
                        {

                            int TID = 0;
                            bool isArray = false;
                            _bs._ltid(this.siaqodb, (int)this.dataGridView1.Rows[e.RowIndex].Cells[0].Value, this.metaType, fi.Name, ref TID, ref isArray);
                            if (TID <= 0)
                            {
                                if (TID == -31)
                                {
                                    e.Value = "[Dictionary<,>]";
                                }
                                else if (TID == -32)
                                {

                                    e.Value = "[Jagged Array]";
                                }
                                else
                                {
                                    e.Value = "[null]";
                                }
                            }
                            else
                            {
                                MetaType mtOfComplex = FindMeta(TID);
                                if (isArray)
                                {
                                    string[] name = mtOfComplex.Name.Split(',');
                                    e.Value = name[0] + " []";
                                }
                                else
                                {
                                    string[] name = mtOfComplex.Name.Split(',');
                                    e.Value = name[0];
                                }
                            }
                        }
                        else
                        {
                            e.Value = siaqodb.LoadValue(oids[e.RowIndex], metaType.Fields[e.ColumnIndex - 1].Name, metaType);
                        }
                        if (e.Value == null)
                        {
                            e.Value = "[null]";
                        }
                    }
                }
            }
		}

		

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
        
        }

        private void dataGridView1_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
        {
            return;
            //problems on MacOSX, this method is never called
            /*
            if (e.ColumnIndex == 0)
            {
                return;
            }
            try
            {

                Sqo.Internal._bs._uf(siaqodb, oids[e.RowIndex], metaType, metaType.Fields[e.ColumnIndex - 1].Name, e.Value);
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
            }*/
        }

        

       
        
       
       
		
	}
    public class MetaEventArgs : EventArgs
    {
        public MetaType mType;
        public List<int> oids;
        public MetaEventArgs(MetaType mType, List<int> oids)
        {
            this.mType = mType;
            this.oids = oids;
        }
    }
}
