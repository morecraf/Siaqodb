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
	[Register ("EncryptionWindowController")]
	partial class EncryptionWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSComboBox AlgorithmCombo { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton EncryptionCheck { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField PasswordText { get; set; }

		[Action ("OnCancel:")]
		partial void OnCancel (MonoMac.Foundation.NSObject sender);

		[Action ("OnOk:")]
		partial void OnOk (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (AlgorithmCombo != null) {
				AlgorithmCombo.Dispose ();
				AlgorithmCombo = null;
			}

			if (EncryptionCheck != null) {
				EncryptionCheck.Dispose ();
				EncryptionCheck = null;
			}

			if (PasswordText != null) {
				PasswordText.Dispose ();
				PasswordText = null;
			}
		}
	}

	[Register ("EncryptionWindow")]
	partial class EncryptionWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
