using System;
using MonoMac.AppKit;
using System.Linq;
using System.Drawing;

namespace SiaqodbManager.Controls
{
	public class LinqTable:NSTableView
	{
		public LinqTable (QueryViewModelAdapter queryView):base(){
			queryView.LinqExecuted += LinqExecuted;
		
			HeaderView.NeedsDisplay = true;
			//table view options
			GridStyleMask = NSTableViewGridStyle.SolidHorizontalLine 
				|NSTableViewGridStyle.SolidVerticalLine;

			this.AutoresizingMask = NSViewResizingMask.WidthSizable |
				NSViewResizingMask.MaxXMargin|
				NSViewResizingMask.MaxYMargin|
				NSViewResizingMask.HeightSizable;
		}
			
		void LinqExecuted (object sender, SiaqodbManager.Entities.LinqEventArgs e)
		{
			while(TableColumns().Length > 0){
				RemoveColumn (TableColumns().Last());
			}
			if(e.DataSource.Count > 0){
				var obj = e.DataSource[0];
				foreach(var property in obj.GetType().GetProperties()){
					var column = new NSTableColumn ();
					column.HeaderCell.Identifier = property.Name;
					column.HeaderCell.Title = property.Name;
					this.AddColumn (column);
				}
				this.DataSource = new LinqDataSource (e.DataSource);
				ReloadData ();
			}
		}
	}
}

