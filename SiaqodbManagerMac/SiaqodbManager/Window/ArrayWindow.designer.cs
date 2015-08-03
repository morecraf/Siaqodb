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
	[Register ("ArrayWindowController")]
	partial class ArrayWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSTextField ArrayText { get; set; }

		[Action ("OnCancel:")]
		partial void OnCancel (MonoMac.Foundation.NSObject sender);

		[Action ("OnSave:")]
		partial void OnSave (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (ArrayText != null) {
				ArrayText.Dispose ();
				ArrayText = null;
			}
		}
	}

	[Register ("ArrayWindow")]
	partial class ArrayWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
