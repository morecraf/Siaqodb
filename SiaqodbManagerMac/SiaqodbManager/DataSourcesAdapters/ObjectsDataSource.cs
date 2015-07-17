using System;
using MonoMac.AppKit;
using MonoMac.Foundation;
using System.Collections.Generic;
using SiaqodbManager.ViewModel;

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

			normalCell = new NSTextFieldCell ();

			headerCell = new NSTableHeaderCell ();
			headerCell.LineBreakMode = NSLineBreakMode.TruncatingMiddle;
			headerCell.FocusRingType = NSFocusRingType.None;
			headerCell.Editable = false;
			headerCell.Selectable = false;
		}

		public void AddResultSet (int rowIndex)
		{
			viewModel.RemoveRow (rowIndex);
		}

//		public void ClearResultSet ()
//		{
//			sections.Clear ();
//		}

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

			if(valueKey != null){
				viewModel.UpdateValue (valueKey,row,theObject);
			}
		}


		public override NSObject GetObjectValue (NSTableView tableView, NSTableColumn tableColumn, int row)
		{
			var valueKey = tableColumn.HeaderCell.Identifier;
			if(valueKey != null){
				var value = viewModel.GetValue(valueKey,row);
				return NSObject.FromObject(value);
			}
			return new NSString("0");
		}
			
	}
}

