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
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSButton ConnectButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSView MainWindow { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField PathInput { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTabView TabView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSOutlineView TypesView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ConnectButton != null) {
				ConnectButton.Dispose ();
				ConnectButton = null;
			}

			if (TabView != null) {
				TabView.Dispose ();
				TabView = null;
			}

			if (MainWindow != null) {
				MainWindow.Dispose ();
				MainWindow = null;
			}

			if (PathInput != null) {
				PathInput.Dispose ();
				PathInput = null;
			}

			if (TypesView != null) {
				TypesView.Dispose ();
				TypesView = null;
			}
		}
	}

	[Register ("MainWindow")]
	partial class MainWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
