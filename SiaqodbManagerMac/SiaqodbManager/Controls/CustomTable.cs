using System;
using MonoMac.AppKit;
using System.Drawing;
using System.Collections.Generic;

namespace SiaqodbManager
{
	public class CustomTable:NSTableView
	{


		public CustomTable ()
		{
			//	SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.None;
			ErrorDict = new Dictionary<Tuple<int,int>,string> ();
		}

		public Dictionary<Tuple<int,int>,string> ErrorDict;
			

		public NSCell CurrentCell { get; set; }

		public void AddErrorForCell (int row,int column,string errorMessage){
			ErrorDict [new Tuple<int,int> (row, column)] = errorMessage;
		}
		public void RemoveErrorForCell(int row,int column){
			ErrorDict.Remove(new Tuple<int,int> (row, column));
		}

		public string ErrorMessage(int row,int column){
			if(ErrorDict.ContainsKey(new Tuple<int,int> (row, column))){
				return ErrorDict[new Tuple<int,int> (row, column)];
			}
			return "";
		}

		public bool HasError(int row,int column){
			if(ErrorDict.ContainsKey(new Tuple<int,int> (row, column))){
				return true;
			}
			return false;
		}

		public NSTableColumn CurrentColumn {
			get;
			set;
		}

		public override void ResetCursorRects ()
		{
			base.ResetCursorRects ();
		}

		public override void MouseDown (NSEvent theEvent)
		{
			DeselectAll (null);
			PointF globalLocation = theEvent.LocationInWindow;
			PointF localLocation = ConvertPointFromView(globalLocation,null);
			var selectedColumn = GetColumn (localLocation);
			var selectedRow = GetRow (localLocation);
			if (selectedRow > -1 && selectedColumn > -1) {
				CurrentCell = GetCell (selectedColumn,selectedRow);
				CurrentColumn = TableColumns()[selectedColumn];
			} else {
				CurrentCell = null;
				CurrentColumn = null;
			}
			base.MouseDown (theEvent);
		}
	}
}

