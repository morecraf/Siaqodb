using System;
using MonoMac.AppKit;
using MonoMac.Foundation;
using System.Collections.Generic;
using SiaqodbManager.ViewModel;
using SiaqodbManager.Repo;
using System.Linq;

namespace SiaqodbManager
{
	public class ObjectsDataSource:NSTableViewDataSource
	{

	
		// Dict key is section name, value is a sorted list of section element (result it comes from and the index in it) with key being the url (to avoid duplicates)
		//Dictionary<string, SortedList<string, Tuple<Result, int>>> sections = new Dictionary<string, SortedList<string, Tuple<Result, int>>> ();
		List<int> data = new List<int> ();

		NSTextFieldCell normalCell;
		NSTableHeaderCell headerCell;
		ObjectViewModelAdapter viewModel;

		public ObjectsDataSource (ObjectViewModelAdapter viewModel)
		{
			this.viewModel = viewModel;


		}

		public void AddNewRow ()
		{
			viewModel.AddNewRow ();
		}

		public void RemoveRow (int rowIndex)
		{
			viewModel.RemoveRow (rowIndex);
		}

		public override int GetRowCount (NSTableView tableView)
		{
			return viewModel.GetNrOfObjects();
		}
			

//		public override NSCell GetCell (NSTableView tableView, NSTableColumn tableColumn, int row)
//		{
//			if (tableView == null)
//				return null;
//			var value = data[row];
//			return !string.IsNullOrEmpty (value) ? headerCell : normalCell;
//		}
			

		public override void SetObjectValue (NSTableView tableView, NSObject theObject, NSTableColumn tableColumn, int row)
		{
			var valueKey = (string)(NSString)tableColumn.HeaderCell.Identifier;
			var columnIndex = 0;
			foreach(var column in tableView.TableColumns()){
				if(column.Equals(tableColumn)){
					break;
				}
				columnIndex++;
			}

			var customTable = tableView as CustomTable;
			if(valueKey != null){
				try{
					viewModel.UpdateValue (valueKey,row,theObject);
					if(customTable != null){
						customTable.RemoveErrorForCell (row, columnIndex);
					}
				}catch(Exception ex){
					if(customTable != null){
						customTable.AddErrorForCell (row, columnIndex,ex.Message);
					}
				}
			}
		}


		public override NSObject GetObjectValue (NSTableView tableView, NSTableColumn tableColumn, int row)
		{
//			if(tableColumn == null){
//				return new NSString("0");
//			}
			var valueKey = tableColumn.HeaderCell.Identifier;
			if(SiaqodbRepo.Opened){
				if(valueKey != null){
					var value = viewModel.GetValue(valueKey,row).ToString();
					return NSObject.FromObject(value);
				}
			}
			return new NSString("0");
		}
			
		public Type GetTypeOfColumn (string identifier)
		{
			return viewModel.Columns[identifier].Item2.FieldType;
		}
	}
}

