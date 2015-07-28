﻿
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


		//BIND THE LOGIN PANEL
		public  override void AwakeFromNib ()
		{
			base.AwakeFromNib ();
			EncryptionWindowController.SetEncryptionSettings ();
			mainViewModel = new MainViewModelAdapter (new MainViewModel());

			BindButton (mainViewModel,"ConnectCommand",ConnectButton);
			PathInput.Bind ("value",mainViewModel,"SelectedPath",BindingUtil.ContinuouslyUpdatesValue);

			TabView.DidSelect += OnTabSelectionChanged;
			AddButton.Activated += OnAddRow;
			RemoveButton.Activated += OnRemoveRow;
			CloseTabButton.Activated += OnCloseTab;

			//types tree view
			TypesView.Delegate = new TypesDelegate (this);
		} 

		public void CreateObjectsTable (MetaTypeViewModelAdapter metaType)
		{
			var tabViewItem = new NSTabViewItem ();
			var tableView = new CustomTable ();
			tableView.Delegate = new ObjectsDelegate (this);


			//table managing section
			var objectAdapter = mainViewModel.CreateObjectsView (metaType);
			ObjectsViewCreator.AddColumnsAndData (tableView,objectAdapter);

			var tableContainer = ObjectsViewCreator.TableActionsLayout (tableView);
			ObjectsViewCreator.CostumizeTable (tableView);

			tabViewItem.Label = metaType.Name;
			tabViewItem.View.AddSubview(tableContainer);

			TablesDictionry[metaType.Name] = tableView;

			TabView.Add(tabViewItem);
			TabView.Select (tabViewItem);
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

		void OnTabSelectionChanged (object sender, NSTabViewItemEventArgs e)
		{
			var label = TabView.Selected.Label;
			if(TablesDictionry.ContainsKey(label)){
				TableActionButtons.Hidden = false;
			}else{
				TableActionButtons.Hidden = true;
			}
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

