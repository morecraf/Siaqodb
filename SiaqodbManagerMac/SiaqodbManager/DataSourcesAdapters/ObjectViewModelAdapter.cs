using System;
using SiaqodbManager.DataSourcesAdapters;
using SiaqodbManager.ViewModel;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;

namespace SiaqodbManager
{
	public class ObjectViewModelAdapter:AbstractViewModelAdapter
	{
		ObjectViewModel viewModel;

		public ObjectViewModelAdapter (ObjectViewModel viewModel):base(viewModel)
		{
			this.viewModel = viewModel;
		}

		public int GetNrOfObjects ()
		{
			return viewModel.NrOFObjects;
		}

		public Dictionary<string,Tuple<int,MetaFieldViewModel>> Columns {
			get{
				return viewModel.ColumnIndexes;
			}
		}

		public void RemoveRow (int rowIndex)
		{
			viewModel.RemoveRow (rowIndex);
		}

		public void AddNewRow ()
		{
			viewModel.AddRow ();
		}

		public object GetValue (string columnName, int rowIndex)
		{
			return viewModel.GetValue (columnName,rowIndex);
		}

		public void UpdateValue(string columnName,int rowIndex,NSObject macValue){
			if(Columns.ContainsKey(columnName)){
				var value = SiaqoMacUtil.FromNSObject (macValue,Columns[columnName].Item2);
				viewModel.UpdateValue (columnName,rowIndex,value);
			}
		}
	}
}

