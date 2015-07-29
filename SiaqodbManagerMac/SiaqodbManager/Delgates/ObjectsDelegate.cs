using System;
using MonoMac.AppKit;
using MonoMac.Foundation;
using System.Drawing;


namespace SiaqodbManager
{
	public class ObjectsDelegate:NSTableViewDelegate
	{
		private ObjectViewModelAdapter viewModel;

		public ObjectsDelegate (ObjectViewModelAdapter viewModel)
		{
			this.viewModel = viewModel;
		}

		public override void SelectionDidChange (NSNotification notification)
		{
			var table = notification.Object as CustomTable;
			if(table.CurrentCell != null && table.CurrentColumn != null){

				var column =  table.CurrentColumn;
				var columnIndex = GetColumnIndex (table,column);
				viewModel.EditComplexObject (table.SelectedRow,columnIndex,column.HeaderCell.Identifier);
			}
		}
			

		public override void WillDisplayCell (NSTableView tableView, MonoMac.Foundation.NSObject cell, NSTableColumn tableColumn, int row)
		{
			var attributedCell = cell as NSCell;
			var objectSource = tableView.DataSource as ObjectsDataSource;
			var customTable = tableView as CustomTable;		

			if(attributedCell != null && objectSource != null){
				attributedCell.SetCellAttribute (NSCellAttribute.CellHighlighted,0);
				var type = objectSource.GetTypeOfColumn (tableColumn.HeaderCell.Identifier);
				if (type == null) {
					var value = attributedCell.Title;
					var attributedValue = new NSMutableAttributedString (value);
					attributedValue.AddAttribute (NSAttributedString.ForegroundColorAttributeName, NSColor.Blue, new NSRange (0, attributedValue.Length));
					attributedValue.AddAttribute (NSAttributedString.CursorAttributeName, NSCursor.PointingHandCursor, new NSRange (0,attributedValue.Length));				
					if (customTable.CurrentCell == attributedCell && row == customTable.SelectedRow) {
						attributedCell.SetCellAttribute (NSCellAttribute.CellHighlighted,1);
					}
					attributedCell.AttributedStringValue = attributedValue;
					attributedCell.Editable = false;
					attributedCell.Selectable = false;
				} else {
					var index = GetColumnIndex (tableView,tableColumn);
					if (index != 0) {
						tableColumn.Editable = true;
					} else {
						tableColumn.Editable = false;
					}
				}
			}
		}
			
		int GetColumnIndex (NSTableView tableView, NSTableColumn tableColumn)
		{
			var i = 0;
			foreach(var column in tableView.TableColumns()){
				if(column.Equals(tableColumn)){
					return i;
				}
				i++;
			}
			return -1;
		}
	}
}

