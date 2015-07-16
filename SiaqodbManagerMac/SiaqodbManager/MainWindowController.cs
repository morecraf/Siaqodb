
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using System.ComponentModel;
using SiaqodbManager.ViewModel;
using SiaqodbManager.DataSourcesAdapters;
using MonoMac.ObjCRuntime;
using SiaqodbManager.Util;

namespace SiaqodbManager
{
	public partial class MainWindowController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors

		// Called when created from unmanaged code
		public MainWindowController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public MainWindowController (NSCoder coder) : base (coder)
		{
			Initialize ();
			Instance = this;
		}
		
		// Call to load from the XIB/NIB file
		public MainWindowController () : base ("MainWindow")
		{
			Initialize ();
			Instance = this;
		}


		
		// Shared initialization code
		void Initialize ()
		{
			Instance = this;
		}

		#endregion
		private MainViewModelAdapter mainViewModel;

		public static MainWindowController Instance{ get; set;}

		public static PropertyChangedEventHandler BindHandler{ get { return Instance.PropretyChangeHandler;} }

		public void PropretyChangeHandler (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			Console.WriteLine (e.PropertyName);

			switch (e.PropertyName) {
				case "TypesList":
					{
						if(mainViewModel != null)
						{
							TypesView.DataSource = new TypesDataSource (mainViewModel.TypesList);
						}
						return;
					}
			}

			NSObject.FromObject (sender).WillChangeValue (e.PropertyName);
			NSObject.FromObject (sender).DidChangeValue (e.PropertyName);
		}

		//BIND THE LOGIN PANEL
		public  override void AwakeFromNib ()
		{
			base.AwakeFromNib ();
			mainViewModel = new MainViewModelAdapter (new MainViewModel());

			BindButton (mainViewModel,"ConnectCommand",ConnectButton);
			PathInput.Bind ("value",mainViewModel,"SelectedPath",BindingUtil.ContinuouslyUpdatesValue);

			//types tree view
			TypesView.Delegate = new TypesDelegate (this);
		} 

		public void CreateObjectsTable (MetaTypeViewModelAdapter metaType)
		{

			var tabViewItem = new NSTabViewItem ();

			var tableScrollView = new NSScrollView ();
			var tableView = new NSTableView ();

			var objectAdapter = mainViewModel.CreateObjectsView (metaType);

			AddColumns (tableView,objectAdapter);

			tableView.DataSource = new ObjectsDataSource (objectAdapter);

			tableScrollView.AutoresizingMask = NSViewResizingMask.HeightSizable 
									| NSViewResizingMask.WidthSizable 
									| NSViewResizingMask.MaxYMargin 
									| NSViewResizingMask.MaxXMargin;
			//table view options
			tableView.GridStyleMask = NSTableViewGridStyle.SolidHorizontalLine 
									|NSTableViewGridStyle.SolidVerticalLine;

			//add all views to their containers
			tableScrollView.ContentView.DocumentView = tableView;
			
			tabViewItem.Label = metaType.Name;
			TabView.Add(tabViewItem);
			tabViewItem.View.AddSubview(tableScrollView);
			TabView.Select (tabViewItem);
		}

		void AddColumns (NSTableView tableView, ObjectViewModelAdapter objectAdapter)
		{
			tableView.HeaderView.NeedsDisplay = true;
			foreach(var columnName in objectAdapter.Columns){
				var tableColumn = new NSTableColumn ();
				tableColumn.HeaderCell.StringValue = columnName;
				tableColumn.HeaderCell.Identifier = columnName;
				tableColumn.Editable = true;
				tableView.AddColumn (tableColumn);
			}
		}

		//strongly typed window accessor
		public new MainWindow Window {
			get {
				return (MainWindow)base.Window;
			}
		}

		private void BindButton(NSObject target,string action,NSButton button){
			button.Target = target;
			button.Action = new Selector (action);
		}
	}
}

