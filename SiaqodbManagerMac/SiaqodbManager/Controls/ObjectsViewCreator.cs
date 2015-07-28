using System;
using MonoMac.AppKit;

namespace SiaqodbManager
{
	public class ObjectsViewCreator
	{
		public ObjectsViewCreator ()
		{
		}

		public static NSView TableActionsLayout (NSTableView tableView)
		{
			var view = new NSView ();
			var scrollView = AddScrollView (tableView);
			view.AddSubview (scrollView);
			view.AutoresizingMask = NSViewResizingMask.HeightSizable 
				| NSViewResizingMask.WidthSizable 
				| NSViewResizingMask.MaxXMargin
				| NSViewResizingMask.MaxYMargin;

			return view;
		}

		public static NSScrollView AddScrollView (NSTableView tableView)
		{
			var tableScrollView = new NSScrollView ();
			tableScrollView.AutoresizingMask = NSViewResizingMask.HeightSizable 
				| NSViewResizingMask.WidthSizable ;
			//add all views to their containers
			tableScrollView.ContentView.DocumentView = tableView;
			return tableScrollView;
		}

		public static void AddColumnsAndData (NSTableView tableView, ObjectViewModelAdapter objectAdapter)
		{
			tableView.HeaderView.NeedsDisplay = true;
			foreach(var columnInfo in objectAdapter.Columns){
				var tableColumn = new NSTableColumn ();
				tableView.AddColumn (tableColumn);
				tableColumn.HeaderCell.Identifier = columnInfo.Key;
				var typeInfo = columnInfo.Value.Item2;
				tableColumn.HeaderCell.StringValue = typeInfo.Name;
			}
			tableView.DataSource = new ObjectsDataSource (objectAdapter);
		}

		public static void CostumizeTable (NSTableView tableView)
		{
			//table view options
			tableView.GridStyleMask = NSTableViewGridStyle.SolidHorizontalLine 
				|NSTableViewGridStyle.SolidVerticalLine;
		}
	}
}

