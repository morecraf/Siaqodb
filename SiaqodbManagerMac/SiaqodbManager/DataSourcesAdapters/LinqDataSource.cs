using System;
using MonoMac.AppKit;
using System.Collections;
using MonoMac.Foundation;

namespace SiaqodbManager
{
	public class LinqDataSource:NSTableViewDataSource
	{
		public IList dataSource;

		public LinqDataSource (IList dataSource)
		{
			this.dataSource = dataSource;
		}

		public override int GetRowCount (NSTableView tableView)
		{
			return dataSource.Count;
		}

		public override NSObject GetObjectValue (NSTableView tableView, NSTableColumn tableColumn, int row)
		{
			var element = dataSource [row];
			var property = element.GetType ().GetProperty (tableColumn.HeaderCell.Identifier);
			return NSObject.FromObject(property.GetValue (element));
		}
	}
}

