using Sqo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SiaqodbManager.ViewModel
{
    public class ObjectViewModel: INotifyPropertyChanged
    {
        private MetaTypeViewModel selectedType;
		private IEnumerable<MetaTypeViewModel> TypesList;
        private Sqo.Siaqodb siaqodb;
		public Dictionary<string,Tuple<int,MetaFieldViewModel>> ColumnIndexes;
       
        private string cell;

		public ObjectViewModel(MetaTypeViewModel SelectedType, IEnumerable<MetaTypeViewModel> TypesList, Sqo.Siaqodb siaqodb)
		{
			// TODO: Complete member initialization
			this.SelectedType = SelectedType;
			this.TypesList = TypesList;
			this.siaqodb = siaqodb;
			oids = siaqodb.LoadAllOIDs(SelectedType.MetaType);

			ColumnIndexes = new Dictionary<string, Tuple<int, MetaFieldViewModel>> ();
			var oidType = new MetaFieldViewModel
			{
				Name ="OID",
				FieldType = typeof(System.Int32),
				ActualName="OID"
			};
			ColumnIndexes ["OID"] = new Tuple<int, MetaFieldViewModel>(0,oidType);
//			Objects = new DataTable();
//			Objects.Columns.Add("OID");

			var index = 1;
			foreach (var field in SelectedType.Fields)
			{
				var tuple = new Tuple<int,MetaFieldViewModel> (index++,field);
				ColumnIndexes[field.ActualName]=tuple;
			}

//			var rowIndex = 0;
//			foreach(var oid in siaqodb.LoadAllOIDs(SelectedType.MetaType)){
//				var row = Objects.NewRow();
//				UpdateCell(SelectedType, siaqodb, rowIndex, row, 0, oidType);
//				var columnIndex = 1;
//				foreach (var field in SelectedType.Fields)
//				{
//					UpdateCell(SelectedType, siaqodb, rowIndex, row, columnIndex, field);
//					columnIndex++;
//				}
//				rowIndex++;
//				Objects.Rows.Add(row);
//			}
//
//			Objects.TableNewRow += OnTableNewRow;
//			Objects.RowChanging += OnRowChange;
//			Objects.RowDeleting += OnDeleteRow;

			OnPropertyChanged("Objects");
		}

		public int NrOFObjects
		{
			get{
				return oids.Count;
			}
		}

		public object GetValue (string columnName, int rowIndex){
			if(ColumnIndexes.ContainsKey(columnName)){
				return GetValue (columnName,rowIndex,ColumnIndexes[columnName].Item2);
			}
			return null;
		}

		public object GetValue (string columnName, int rowIndex,MetaFieldViewModel field)
		{
            object value;
			var columnIndex = 0;


			if(ColumnIndexes.ContainsKey(columnName)){
				columnIndex = ColumnIndexes[columnName].Item1;
			}
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
					Sqo.Internal._bs._ltid(siaqodb, Convert.ToInt32(oids[rowIndex]), SelectedType.MetaType, field.ActualName, ref TID, ref isArray);
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
			return value;
		}
     




//        private void UpdateCell(MetaTypeViewModel SelectedType, Sqo.Siaqodb siaqodb, int rowIndex, DataRow row, int columnIndex, MetaFieldViewModel field)
//        {
//            object value;
//            if (columnIndex == 0)
//            {
//                value = oids[rowIndex];
//            }
//            else
//            {
//                if (field.FieldType == null)//complex type
//                {
//                    int TID = 0;
//                    bool isArray = false;
//                    Sqo.Internal._bs._ltid(siaqodb, Convert.ToInt32(row.ItemArray[0]), SelectedType.MetaType, field.ActualName, ref TID, ref isArray);
//                    if (TID <= 0)
//                    {
//                        if (TID == -31)
//                        {
//                            value = "[Dictionary<,>]";
//                        }
//                        else if (TID == -32)
//                        {
//
//                            value = "[Jagged Array]";
//                        }
//                        else
//                        {
//                            value = "[null]";
//                        }
//                    }
//                    else
//                    {
//                        MetaType mtOfComplex = FindMeta(TID);
//                        if (isArray)
//                        {
//                            string[] name = mtOfComplex.Name.Split(',');
//                            value = name[0] + " []";
//                        }
//                        else
//                        {
//                            string[] name = mtOfComplex.Name.Split(',');
//                            value = name[0];
//                        }
//                    }
//                }
//                else
//                {
//                    value = siaqodb.LoadValue(oids[rowIndex], field.ActualName, SelectedType.MetaType);
//                }
//                if (value == null)
//                {
//                    value = "[null]";
//                }
//            }
//            row[field.Name] = value;
//        }

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

        internal void UpdateValue(string columnName, int rowIndex, object value)
        {
            var metaType = SelectedType.MetaType;
            Sqo.Internal._bs._uf(siaqodb, oids[rowIndex], metaType, columnName, value);
        }

        internal void RemoveRow(int index)
        {
            int oid = (int)oids[index];
            Sqo.Internal._bs._do(siaqodb, oid, SelectedType.MetaType);
            oids.RemoveAt(index);
        }
    }
}
