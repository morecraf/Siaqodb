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
		MonoMac.AppKit.NSButton AddButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CloseTabButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton ConnectButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton ExecuteButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton LinqButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSView MainWindow { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField PathInput { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton RemoveButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSView TableActionButtons { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTabView TabView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSOutlineView TypesView { get; set; }

		[Action ("OnEncryption:")]
		partial void OnEncryption (MonoMac.Foundation.NSObject sender);

		[Action ("OnReferences:")]
		partial void OnReferences (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (AddButton != null) {
				AddButton.Dispose ();
				AddButton = null;
			}

			if (CloseTabButton != null) {
				CloseTabButton.Dispose ();
				CloseTabButton = null;
			}

			if (ConnectButton != null) {
				ConnectButton.Dispose ();
				ConnectButton = null;
			}

			if (LinqButton != null) {
				LinqButton.Dispose ();
				LinqButton = null;
			}

			if (MainWindow != null) {
				MainWindow.Dispose ();
				MainWindow = null;
			}

			if (PathInput != null) {
				PathInput.Dispose ();
				PathInput = null;
			}

			if (RemoveButton != null) {
				RemoveButton.Dispose ();
				RemoveButton = null;
			}

			if (ExecuteButton != null) {
				ExecuteButton.Dispose ();
				ExecuteButton = null;
			}

			if (TableActionButtons != null) {
				TableActionButtons.Dispose ();
				TableActionButtons = null;
			}

			if (TabView != null) {
				TabView.Dispose ();
				TabView = null;
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
