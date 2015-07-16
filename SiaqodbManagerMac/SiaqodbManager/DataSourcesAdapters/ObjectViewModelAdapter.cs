using System;
using SiaqodbManager.DataSourcesAdapters;
using SiaqodbManager.ViewModel;
using System.Collections.Generic;
using System.Linq;

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

		public List<string> Columns {
			get{
				return viewModel.ColumnIndexes.Keys.ToList();
			}
		}

		public object GetValue (string columnName, int rowIndex)
		{
			return viewModel.GetValue (columnName,rowIndex);
		}
	}
}

