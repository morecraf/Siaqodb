using System;
using MonoMac.AppKit;
using SiaqodbManager.Util;
using SiaqodbManager.CustomWindow;
using SiaqodbManager.Controls;

namespace SiaqodbManager
{
	public class LinqTabBuilder
	{
		public LinqTabBuilder ()
		{
		}

		public static NSTabViewItem BuildTab (QueryViewModelAdapter queryViewModel)
		{
			var tabViewItem = new NSTabViewItem ();
			var queryView = new NSSplitView ();
			var scrolView = new NSScrollView ();
			var documentScrollView = new NSScrollView ();
			var documentView = new DocumentTextView ();
			var tableView = new LinqTable (queryViewModel);
			var scrollMessage = new NSScrollView ();
			var messageView = new ErrorTextView ();

			var resultTab = new NSTabView ();
			var tableTab = new NSTabViewItem ();
			var messageTab = new NSTabViewItem ();

			FillParentVIew (resultTab);
			FillParentVIew (scrolView);
			FillParentVIew (queryView);
			FillParentVIew (messageView);
			FillParentVIew (scrolView);
			FillParentVIew (scrollMessage);

			documentView.Bind ("attributedString", queryViewModel, "Linq", BindingUtil.ContinuouslyUpdatesValue);
			queryViewModel.ErrorOccured += messageView.ErrorOccured;

			documentScrollView.ContentView.DocumentView = documentView;
			scrolView.ContentView.DocumentView = tableView;
			scrollMessage.ContentView.DocumentView = messageView;

			tableTab.View.AddSubview (scrolView);
			tableTab.Label = "Result";

			resultTab.Add (tableTab);
			resultTab.Add (messageTab);

			messageTab.Label = "Message";
			messageTab.View.AddSubview (scrollMessage);

			queryView.AddSubview (documentScrollView);
			queryView.AddSubview (resultTab);

			tabViewItem.View.AddSubview (queryView);
			tabViewItem.Label = "New linq doc";

			return tabViewItem;
		}

		static void FillParentVIew (NSView queryView)
		{
			queryView.AutoresizingMask = NSViewResizingMask.MaxXMargin | NSViewResizingMask.MaxYMargin | NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable;
		}
	}
}

