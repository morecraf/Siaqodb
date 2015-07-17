
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
			EncryptionWindowController.SetEncryptionSettings ();
			mainViewModel = new MainViewModelAdapter (new MainViewModel());

			BindButton (mainViewModel,"ConnectCommand",ConnectButton);
			PathInput.Bind ("value",mainViewModel,"SelectedPath",BindingUtil.ContinuouslyUpdatesValue);

			//types tree view
			TypesView.Delegate = new TypesDelegate (this);
		} 

		public void CreateObjectsTable (MetaTypeViewModelAdapter metaType)
		{
			var tabViewItem = new NSTabViewItem ();
			var tableView = new NSTableView ();
			var tableController = new NSArrayController ();

			//table managing section
			var objectAdapter = mainViewModel.CreateObjectsView (metaType);
			ObjectsViewCreator.AddColumnsAndData (tableView,objectAdapter);
			var tableContainer = ObjectsViewCreator.TableActionsLayout (tableView,tableController);
			ObjectsViewCreator.CostumizeTable (tableView);

			tabViewItem.Label = metaType.Name;
			tabViewItem.View.AddSubview(tableContainer);

			TabView.Add(tabViewItem);
			TabView.Select (tabViewItem);
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

