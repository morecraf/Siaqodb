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
			mainWindowController.Window.MakeKeyAndOrderFront (this);
		}
	}
}

