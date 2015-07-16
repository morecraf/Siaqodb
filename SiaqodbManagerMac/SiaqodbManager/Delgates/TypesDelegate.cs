using System;
using MonoMac.AppKit;
using SiaqodbManager.DataSourcesAdapters;

namespace SiaqodbManager
{
	public class TypesDelegate:NSOutlineViewDelegate
	{
		MainWindowController mainController;

		public TypesDelegate (MainWindowController mainController)
		{
			this.mainController = mainController;
		}

		public override void SelectionDidChange (MonoMac.Foundation.NSNotification notification)
		{
			if(notification.Object is NSOutlineView){
				var outlineView = notification.Object as NSOutlineView;
				var indexes = outlineView.SelectedRows;
				var selectedItem = outlineView.ItemAtRow((int)indexes.FirstIndex);

				if(selectedItem is MetaTypeViewModelAdapter){
					mainController.CreateObjectsTable (selectedItem as MetaTypeViewModelAdapter);
				}
			}
		}
	}
}

