
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
using SiaqodbManager.Entities;
using SiaqodbManager.Controls;
using SiaqodbManager.CustomWindow;

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
		}
		
		// Call to load from the XIB/NIB file
		public MainWindowController () : base ("MainWindow")
		{
			Initialize ();
		}


		
		// Shared initialization code
		void Initialize ()
		{
			Instance = this;
			TablesDictionry = new Dictionary<string, NSTableView> ();
		}

		#endregion
		private MainViewModelAdapter mainViewModel;

		public static MainWindowController Instance{ get; set;}

		public static PropertyChangedEventHandler BindHandler{ get { return Instance.PropretyChangeHandler;} }

		private Dictionary<string,NSTableView> TablesDictionry; 


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

		partial void OnEncryption (NSObject sender)
		{
			EncryptionViewModel.Instance.Parent = mainViewModel;
			var controller = new EncryptionWindowController ();
			NSApplication.SharedApplication.RunModalForWindow(controller.Window);
		}

		partial void OnReferences (NSObject sender)
		{
			var controller = new ReferenceWindowController ();
			NSApplication.SharedApplication.RunModalForWindow(controller.Window);
		}


		//BIND THE LOGIN PANEL
		public  override void AwakeFromNib ()
		{
			base.AwakeFromNib ();
			EncryptionViewModel.Instance.SetEncryptionSettings ();
			mainViewModel = new MainViewModelAdapter (new MainViewModel());

			BindButton (mainViewModel,"ConnectCommand",ConnectButton);
			PathInput.Bind ("value",mainViewModel,"SelectedPath",BindingUtil.ContinuouslyUpdatesValue);

			TabView.DidSelect += OnTabSelectionChanged;
			AddButton.Activated += OnAddRow;
			RemoveButton.Activated += OnRemoveRow;
			CloseTabButton.Activated += OnCloseTab;
			LinqButton.Activated += OnLinqTab;

			//types tree view
			TypesView.Delegate = new TypesDelegate (this);
		} 

		public void CreateObjectsTable (MetaTypeViewModelAdapter metaType)
		{
			CreateObjectsTable (metaType,null);
		}

		void CreateObjectsTable (MetaTypeViewModelAdapter metaType,List<int> oids)
		{
			var tabViewItem = new NSTabViewItem ();
			var tableView = new CustomTable ();
			//table managing section
			var objectAdapter = mainViewModel.CreateObjectsView (metaType, oids);
			tableView.Delegate = new ObjectsDelegate (objectAdapter);
			objectAdapter.OpenObjects += OpenObjects;
			ObjectsViewCreator.AddColumnsAndData (tableView, objectAdapter);
			var tableContainer = ObjectsViewCreator.TableActionsLayout (tableView);
			ObjectsViewCreator.CostumizeTable (tableView);
			tabViewItem.Label = metaType.Name;
			tabViewItem.View.AddSubview (tableContainer);
			TablesDictionry [metaType.Name] = tableView;
			TabView.Add (tabViewItem);
			TabView.Select (tabViewItem);
		}

		public void OpenObjects (object sender,MetaEventArgs e)
		{
			CreateObjectsTable (new MetaTypeViewModelAdapter(new MetaTypeViewModel(e.mType)),e.oids);
		}


		//strongly typed window accessor
		public new MainWindow Window {
			get {
				return (MainWindow)base.Window;
			}
		}

		void OnAddRow (object sender, EventArgs e)
		{
			var tableView = TablesDictionry[TabView.Selected.Label]; 
			var dataSource = tableView.DataSource as ObjectsDataSource;

			dataSource.AddNewRow ();
			tableView.ReloadData ();
		}

		void OnCloseTab (object sender, EventArgs e)
		{
			var tab = TabView.Selected;
			TabView.Remove (tab);
		}

		void OnLinqTab (object sender, EventArgs e)
		{
			var tabViewItem = new NSTabViewItem ();
			var queryView = new NSSplitView ();
			queryView.AutoresizingMask = NSViewResizingMask.MaxXMargin|
				NSViewResizingMask.MaxYMargin|
				NSViewResizingMask.HeightSizable|
				NSViewResizingMask.WidthSizable;

			var scrolView = new NSScrollView ();
			var documentScrollView = new NSScrollView ();

			var queryViewModel = mainViewModel.CreateQueryView (new SaveFileService());
			var documentView = new DocumentTextView ();
			documentView.Bind ("attributedString",queryViewModel,"Linq",BindingUtil.ContinuouslyUpdatesValue);
			BindButton (queryViewModel,"ExecuteCommand",ExecuteButton);
			documentScrollView.ContentView.DocumentView = documentView;

			var tableView = new LinqTable (queryViewModel);
			scrolView.ContentView.DocumentView = tableView;

			queryView.AddSubview (documentScrollView);
			queryView.AddSubview (scrolView);
			tabViewItem.View.AddSubview (queryView);

			tabViewItem.Label = "New linq doc";
			TabView.Add (tabViewItem);
			TabView.Select (tabViewItem);
		}

		void OnTabSelectionChanged (object sender, NSTabViewItemEventArgs e)
		{
			var label = TabView.Selected.Label;
			if(TablesDictionry.ContainsKey(label)){
				TableActionButtons.Hidden = false;
			}else{
				TableActionButtons.Hidden = true;
			}
		}

		void LinqExecuted (object sender, LinqEventArgs e)
		{

		}

		void OnRemoveRow (object sender, EventArgs e)
		{
			var tableView = TablesDictionry[TabView.Selected.Label]; 
			var dataSource = tableView.DataSource as ObjectsDataSource;

			int rowIndex = tableView.SelectedRow;

			if(rowIndex > -1){
				dataSource.RemoveRow (rowIndex);
				tableView.ReloadData ();
			}
		}

		private void BindButton(NSObject target,string action,NSButton button){
			button.Target = target;
			button.Action = new Selector (action);
		}
	}
}

