using System;
using MonoMac.AppKit;
using System.Linq;

namespace SiaqodbManager.Controls
{
	public class LinqTable:NSTableView
	{
		public LinqTable (QueryViewModelAdapter queryView):base(){
			queryView.LinqExecuted += LinqExecuted;
			SetFrameSize (new System.Drawing.SizeF(100,100));

			SetFrameOrigin (new System.Drawing.PointF(100,100));		
//			this.AutoresizingMask = NSViewResizingMask.WidthSizable |
//				NSViewResizingMask.MaxXMargin|
//				NSViewResizingMask.MaxYMargin|
//				NSViewResizingMask.HeightSizable;
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

