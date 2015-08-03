using System;

namespace SiaqodbManager.Model
{
	public class ArrayEditArgs:EventArgs
	{
		public string ColumnName{ get; set;}
		public int RowIndex{ get; set;}
		public int ColumnIndex{ get; set; }

		public ObjectViewModelAdapter ViewModel{ get; set;}

		public ArrayEditArgs ()
		{
		}
	}
}

