using Sqo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SiaqodbManager.ViewModel
{
    public class ObjectViewModel: INotifyPropertyChanged
    {
        private MetaTypeViewModel selectedType;
        private System.Collections.ObjectModel.ObservableCollection<MetaTypeViewModel> TypesList;
        private Sqo.Siaqodb siaqodb;
        private DataTable objects;
        private string cell;

       public DataTable Objects {
            get {
                return objects;
            }
            set
            {
                objects = value;
                OnPropertyChanged();
            }
        }

        

        public ObjectViewModel(MetaTypeViewModel SelectedType, System.Collections.ObjectModel.ObservableCollection<MetaTypeViewModel> TypesList, Sqo.Siaqodb siaqodb)
        {
            // TODO: Complete member initialization
            this.SelectedType = SelectedType;
            this.TypesList = TypesList;
            this.siaqodb = siaqodb;
            oids = siaqodb.LoadAllOIDs(SelectedType.MetaType);
            Objects = new DataTable();
            Objects.Columns.Add("OID");
            foreach (var field in SelectedType.Fields)
            {
                Objects.Columns.Add(field.Name);
            }
            var rowIndex = 0;
            var oidType = new MetaFieldViewModel
                {
                    Name ="OID",
                    FieldType = typeof(System.Int32),
                    ActualName="OID"
                };
            foreach(var oid in siaqodb.LoadAllOIDs(SelectedType.MetaType)){
                var row = Objects.NewRow();
                UpdateCell(SelectedType, siaqodb, rowIndex, row, 0, oidType);
                var columnIndex = 1;
                foreach (var field in SelectedType.Fields)
                {
                    UpdateCell(SelectedType, siaqodb, rowIndex, row, columnIndex, field);
                    columnIndex++;
                }
                rowIndex++;
                Objects.Rows.Add(row);
            }

            Objects.TableNewRow += OnTableNewRow;
            Objects.RowChanging += OnRowChange;
            Objects.RowDeleting += OnDeleteRow;

            OnPropertyChanged("Objects");
        }

        private void OnDeleteRow(object sender, DataRowChangeEventArgs e)
        {
            //if (e.Row.Cells[0].Value is int)
            //{
            //    if (MessageBox.Show("Are you sure to delete this object?", "", MessageBoxButton.YesNo) == MessageBoxResult.No)
            //    {
            //        e.Cancel = true;
            //    }
            //    else
            //    {
            var oid = Convert.ToInt32(e.Row.ItemArray[0]);
            Sqo.Internal._bs._do(siaqodb, oid, SelectedType.MetaType);
            oids.Remove(oid);

            //    }
            //}
            //else//is new
            //{
            //    int oid = oids[oids.Count - 1];
            //    Sqo.Internal._bs._do(siaqodb, oid, metaType);
            //    oids.Remove(oid);
            //}
        }

        private void OnRowChange(object sender, DataRowChangeEventArgs e)
        {
            if (e.Action == DataRowAction.Add)
            {
                var row = Objects.Rows[objects.Rows.Count];
                int oid = Sqo.Internal._bs._io(siaqodb, SelectedType.MetaType);
                this.oids.Add(oid);
                //row.ItemArray[0] = oid;
                //row.BeginEdit();
                //row["OID"] = oid;
                //Objects.Rows.Add(row);
                //row.EndEdit();
                //row.AcceptChanges();
            }
        }

        private void OnTableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            // OnPropertyChanged("Objects");
            //var row = e.Row;
            //int oid = Sqo.Internal._bs._io(siaqodb, SelectedType.MetaType);
            //this.oids.Add(oid);
            //row.BeginEdit();
            //row["OID"] = oid;
            //row.EndEdit();
            //row.AcceptChanges();
        }

        private void UpdateCell(MetaTypeViewModel SelectedType, Sqo.Siaqodb siaqodb, int rowIndex, DataRow row, int columnIndex, MetaFieldViewModel field)
        {
            object value;
            if (columnIndex == 0)
            {
                value = oids[rowIndex];
            }
            else
            {
                if (field.FieldType == null)//complex type
                {
                    int TID = 0;
                    bool isArray = false;
                    Sqo.Internal._bs._ltid(siaqodb, Convert.ToInt32(row.ItemArray[0]), SelectedType.MetaType, field.ActualName, ref TID, ref isArray);
                    if (TID <= 0)
                    {
                        if (TID == -31)
                        {
                            value = "[Dictionary<,>]";
                        }
                        else if (TID == -32)
                        {

                            value = "[Jagged Array]";
                        }
                        else
                        {
                            value = "[null]";
                        }
                    }
                    else
                    {
                        MetaType mtOfComplex = FindMeta(TID);
                        if (isArray)
                        {
                            string[] name = mtOfComplex.Name.Split(',');
                            value = name[0] + " []";
                        }
                        else
                        {
                            string[] name = mtOfComplex.Name.Split(',');
                            value = name[0];
                        }
                    }
                }
                else
                {
                    value = siaqodb.LoadValue(oids[rowIndex], field.ActualName, SelectedType.MetaType);
                }
                if (value == null)
                {
                    value = "[null]";
                }
            }
            row[field.Name] = value;
        }

        private MetaType FindMeta(int TID)
        {
            return TypesList.Select(m=>m.MetaType).First<MetaType>(tii => tii.TypeID == TID);
        }
    
        public event PropertyChangedEventHandler PropertyChanged;

        public MetaTypeViewModel SelectedType
        {
            get
            {
                return selectedType;
            }
            set
            {
                selectedType = value;
                OnPropertyChanged();
            }
        }

        private List<int> oids;

        //EVENT HANDLER
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void CellChanged(object sender, EventArgs e)
        {
            
        }
    }
}
