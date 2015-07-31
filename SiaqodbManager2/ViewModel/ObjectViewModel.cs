using SiaqodbManager.Entities;
using Sqo;
using Sqo.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using SiaqodbManager.Entities;
using MonoMac.AppKit;
using Sqo.Exceptions;

namespace SiaqodbManager.ViewModel
{
    public class ObjectViewModel: INotifyPropertyChanged
    {
        private MetaTypeViewModel selectedType;
		private IEnumerable<MetaTypeViewModel> TypesList;
 
		public Dictionary<string,Tuple<int,MetaFieldViewModel>> ColumnIndexes;
       
        private string cell;

		public ObjectViewModel(MetaTypeViewModel SelectedType, IEnumerable<MetaTypeViewModel> TypesList):this(SelectedType,TypesList,null)
        {

        }
		public ObjectViewModel(MetaTypeViewModel SelectedType, IEnumerable<MetaTypeViewModel> TypesList,List<int> oidsToShow)
		{
			// TODO: Complete member initialization
			this.SelectedType = SelectedType;
			this.TypesList = TypesList;

            if(oidsToShow == null || oidsToShow.Count ==0){
				oids = SiaqodbRepo.Instance.LoadAllOIDs(SelectedType.MetaType);
            }
            else
            {
                oids = oidsToShow;
            }

			ColumnIndexes = new Dictionary<string, Tuple<int, MetaFieldViewModel>> ();
			var oidType = new MetaFieldViewModel
			{
				Name ="OID",
				FieldType = typeof(System.Int32),
				ActualName="OID"
			};
			ColumnIndexes ["OID"] = new Tuple<int, MetaFieldViewModel>(0,oidType);

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
			try{
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
						Sqo.Internal._bs._ltid(SiaqodbRepo.Instance, Convert.ToInt32(oids[rowIndex]), SelectedType.MetaType, field.ActualName, ref TID, ref isArray);
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
						value = SiaqodbRepo.Instance.LoadValue(oids[rowIndex], field.ActualName, SelectedType.MetaType);
	                }
	                if (value == null)
	                {
	                    value = "[null]";
	                }
	            }
				return value;
			}catch(SiaqodbException ex){
				SiaqodbRepo.Dispose ();
				throw ex;
			}
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
			Sqo.Internal._bs._uf(SiaqodbRepo.Instance, oids[rowIndex], metaType, columnName, value);
			var alert = new NSAlert {
				MessageText = value.ToString(),
				AlertStyle = NSAlertStyle.Informational
			};

			alert.AddButton ("OK");
			alert.AddButton ("Cancel");

			var returnValue = alert.RunModal();

        }

        internal void RemoveRow(int index)
        {
			try{
	            int oid = (int)oids[index];
				Sqo.Internal._bs._do(SiaqodbRepo.Instance, oid, SelectedType.MetaType);
	            oids.RemoveAt(index);
			}catch(Exception ex){
				SiaqodbRepo.Dispose ();
				throw ex;
			}
        }

        internal void AddRow()
        {
			try
			{
				int oid = Sqo.Internal._bs._io(SiaqodbRepo.Instance, SelectedType.MetaType);
	            this.oids.Add(oid);
			}catch(Exception ex){
				SiaqodbRepo.Dispose ();
				throw ex;
			}
        }

        internal void EditComplexObject(int rowIndex,int columnIndex, string columnName)
        {
			try{
	            MetaField fi = SelectedType.MetaType.Fields[columnIndex-1];

	            List<int> selectedOids = new List<int>();
	            int TID=0;
	            var value = GetValue(columnName,rowIndex);
	            if (value == "[null]")
	            {
	                return;
	            }
	            if (oids[0] != null)//is not new row
	            {
					_bs._loidtid(SiaqodbRepo.Instance, Convert.ToInt32(oids[0]), SelectedType.MetaType, fi.Name, ref selectedOids, ref TID);
	                if (selectedOids.Count == 0 || TID <= 0)
	                {

	                }
	                else
	                {
	                    MetaType mtOfComplex = FindMeta(TID);
	                    OnOpenObjects(new MetaEventArgs(mtOfComplex, selectedOids));
	                }
	            }
			}catch(Exception ex){
				SiaqodbRepo.Dispose ();
				throw ex;
			}
        }
        protected void OnOpenObjects(MetaEventArgs args)
        {
            if (this.OpenObjects != null)
            {
                this.OpenObjects(this, args);
            }
        }

        public EventHandler<MetaEventArgs> OpenObjects;
    }
}
