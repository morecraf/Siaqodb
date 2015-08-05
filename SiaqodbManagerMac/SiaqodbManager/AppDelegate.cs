using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using SiaqodbManager.DataSourcesAdapters;
using SiaqodbManager.ViewModel;

namespace SiaqodbManager
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		MainWindowController mainWindowController;

		public AppDelegate ()
		{
		}
		public override void FinishedLaunching (NSObject notification)
		{
			var mainViewModel = new MainViewModelAdapter (new MainViewModel());

			mainWindowController = new MainWindowController (mainViewModel);

			EncryptionMenu.Activated += mainWindowController.OnEncryption;
			ReferenceMenu.Activated += mainWindowController.OnReference;
			NewLinqMenu.Activated += mainWindowController.OnLinqTab;
			SaveAsMenu.Activated += mainWindowController.OnSaveLinq;
			ExecuteMenu.Activated += mainWindowController.OnExecuteLinq;
			OpenLinqMenu.Activated += mainWindowController.OnOpenLinq;

			mainWindowController.RegisterLinqAction (SaveAsMenu);
			mainWindowController.RegisterLinqAction (ExecuteMenu);

			mainWindowController.Window.MakeKeyAndOrderFront (this);
		}
	}
}

