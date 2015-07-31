// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

namespace SiaqodbManager
{
	[Register ("AppDelegate")]
	partial class AppDelegate
	{
		[Outlet]
		MonoMac.AppKit.NSMenuItem EncryptionMenu { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem ExecuteMenu { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem ReferenceMenu { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ExecuteMenu != null) {
				ExecuteMenu.Dispose ();
				ExecuteMenu = null;
			}

			if (EncryptionMenu != null) {
				EncryptionMenu.Dispose ();
				EncryptionMenu = null;
			}

			if (ReferenceMenu != null) {
				ReferenceMenu.Dispose ();
				ReferenceMenu = null;
			}
		}
	}
}
