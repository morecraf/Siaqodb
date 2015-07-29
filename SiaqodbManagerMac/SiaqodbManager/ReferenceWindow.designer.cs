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
	[Register ("ReferenceWindowController")]
	partial class ReferenceWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSButton AddButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton AddDefaultButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton OkButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton OKButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSArrayController ReferencesArray { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton RemoveButton { get; set; }

		[Action ("OnCancel:")]
		partial void OnCancel (MonoMac.Foundation.NSObject sender);

		[Action ("OnLoadReferences:")]
		partial void OnLoadReferences (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (OkButton != null) {
				OkButton.Dispose ();
				OkButton = null;
			}

			if (ReferencesArray != null) {
				ReferencesArray.Dispose ();
				ReferencesArray = null;
			}

			if (AddButton != null) {
				AddButton.Dispose ();
				AddButton = null;
			}

			if (AddDefaultButton != null) {
				AddDefaultButton.Dispose ();
				AddDefaultButton = null;
			}

			if (OKButton != null) {
				OKButton.Dispose ();
				OKButton = null;
			}

			if (RemoveButton != null) {
				RemoveButton.Dispose ();
				RemoveButton = null;
			}
		}
	}

	[Register ("ReferenceWindow")]
	partial class ReferenceWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
