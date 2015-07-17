
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Sqo;

namespace SiaqodbManager
{
	public partial class EncryptionWindowController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors

		// Called when created from unmanaged code
		public EncryptionWindowController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public EncryptionWindowController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Call to load from the XIB/NIB file
		public EncryptionWindowController () : base ("EncryptionWindow")
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
		}

		#endregion

		public static void SetEncryptionSettings()
		{
			SiaqodbConfigurator.EncryptedDatabase = true;
			if (SiaqodbConfigurator.EncryptedDatabase)
			{
				SiaqodbConfigurator.SetEncryptor(BuildInAlgorithm.AES);

				if (!string.IsNullOrEmpty(""))
				{
					SiaqodbConfigurator.SetEncryptionPassword("");

				}

			}
		}

		//strongly typed window accessor
		public new EncryptionWindow Window {
			get {
				return (EncryptionWindow)base.Window;
			}
		}
	}
}

