using System;
using MonoMac.AppKit;
using System.Drawing;

namespace SiaqodbManager
{
	public class CustomTable:NSTableView
	{

		public CustomTable ()
		{
			SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.None;
		}
			

		public NSCell CurrentCell { get; set; }

		public override void MouseDown (NSEvent theEvent)
		{
			PointF globalLocation = theEvent.LocationInWindow;
			PointF localLocation = ConvertPointFromView(globalLocation,null);
			var selectedColumn = GetColumn (localLocation);
			var selectedRow = GetRow (localLocation);
			if (selectedRow > -1 && selectedColumn > -1) {
				CurrentCell = GetCell (selectedColumn,selectedRow);
			} else {
				CurrentCell = null;
			}
			base.MouseDown (theEvent);
		}
	}
}

