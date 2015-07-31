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
		MonoMac.AppKit.NSMenuItem NewLinqMenu { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem OpenLinqMenu { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem ReferenceMenu { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem SaveAsMenu { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem SaveMenu { get; set; }

		[Action ("NewLinq:")]
		partial void NewLinq (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (EncryptionMenu != null) {
				EncryptionMenu.Dispose ();
				EncryptionMenu = null;
			}

			if (ExecuteMenu != null) {
				ExecuteMenu.Dispose ();
				ExecuteMenu = null;
			}

			if (OpenLinqMenu != null) {
				OpenLinqMenu.Dispose ();
				OpenLinqMenu = null;
			}

			if (SaveMenu != null) {
				SaveMenu.Dispose ();
				SaveMenu = null;
			}

			if (SaveAsMenu != null) {
				SaveAsMenu.Dispose ();
				SaveAsMenu = null;
			}

			if (ReferenceMenu != null) {
				ReferenceMenu.Dispose ();
				ReferenceMenu = null;
			}

			if (NewLinqMenu != null) {
				NewLinqMenu.Dispose ();
				NewLinqMenu = null;
			}
		}
	}
}
