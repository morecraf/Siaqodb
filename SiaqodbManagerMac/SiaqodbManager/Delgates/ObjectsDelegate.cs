using System;
using MonoMac.AppKit;
using MonoMac.Foundation;
using System.Drawing;
using SiaqodbManager.Model;
using System.Collections;
using MonoMac.CoreGraphics;


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
			if(table.SelectedRow < 0){
				return;
			}
			if(table.CurrentCell != null && table.CurrentColumn != null){
				var column =  table.CurrentColumn;
				var columnIndex = GetColumnIndex (table,column);
				if(viewModel.Columns[column.HeaderCell.Identifier].Item2.FieldType == null){
					viewModel.EditComplexObject (table.SelectedRow,columnIndex,column.HeaderCell.Identifier);
				}else{
					OnArrayClicked (column.HeaderCell.Identifier,table.SelectedRow,columnIndex);
					//viewModel.
				}
			}
		}
			

		public event EventHandler<ArrayEditArgs> ArrayClicked;

		public void OnArrayClicked(string column,int row,int columnIndex){
		
			if(ArrayClicked != null){
				ArrayClicked (this,new ArrayEditArgs{
					ColumnIndex = columnIndex,
					ColumnName = column,
					RowIndex = row,
					ViewModel = viewModel
				});
			}
		}


		public override void WillDisplayCell (NSTableView tableView, MonoMac.Foundation.NSObject cell, NSTableColumn tableColumn, int row)
		{
			var attributedCell = cell as NSTextFieldCell;
			var objectSource = tableView.DataSource as ObjectsDataSource;
			var customTable = tableView as CustomTable;		
			var columnIndex = GetColumnIndex (tableView,tableColumn);


			if(attributedCell != null && objectSource != null){
				if(customTable.HasError(row,columnIndex)){
					//var error = 
					//attributedCell.SetCellAttribute (NSCellAttribute.ChangeBackgroundCell,1);
					//attributedCell.ControlView.Layer.BorderColor = new CGColor(255,0,0);
					attributedCell.DrawsBackground = true;
					attributedCell.BackgroundColor = NSColor.Red;
					//attributedCell.
				}
				attributedCell.SetCellAttribute (NSCellAttribute.CellHighlighted,0);
				var type = objectSource.GetTypeOfColumn (tableColumn.HeaderCell.Identifier);
				if (type == null || typeof(IList).IsAssignableFrom(type)) {
					var value = attributedCell.Title;
					var attributedValue = new NSMutableAttributedString (value);

					//tableView.AddCursorRect (attributedCell.ControlView.Frame,NSCursor.PointingHandCursor);

					attributedValue.AddAttribute (NSAttributedString.ForegroundColorAttributeName, NSColor.Blue, new NSRange (0, attributedValue.Length));
					attributedValue.AddAttribute (NSAttributedString.CursorAttributeName, NSCursor.PointingHandCursor, new NSRange (0,attributedValue.Length));				
					if (customTable.CurrentCell == attributedCell && row == customTable.SelectedRow) {
						attributedCell.SetCellAttribute (NSCellAttribute.CellHighlighted,1);
					}
					attributedCell.AttributedStringValue = attributedValue;
					attributedCell.Editable = false;
					attributedCell.Selectable = false;
				} else {
					if (columnIndex != 0) {
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

