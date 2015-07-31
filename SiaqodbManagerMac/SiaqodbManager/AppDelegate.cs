using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

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
			mainWindowController = new MainWindowController ();

			EncryptionMenu.Activated += mainWindowController.OnEncryption;
			ReferenceMenu.Activated += mainWindowController.OnReference;
			NewLinqMenu.Activated += mainWindowController.OnLinqTab;
			SaveAsMenu.Activated += mainWindowController.OnSaveLinq;
			ExecuteMenu.Activated += mainWindowController.OnExecuteLinq;
			OpenLinqMenu.Activated += mainWindowController.OnOpenLinq;

			mainWindowController.Window.MakeKeyAndOrderFront (this);
		}
	}
}

