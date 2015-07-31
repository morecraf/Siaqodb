
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Sqo;
using SiaqodbManager.DataSourcesAdapters;
using SiaqodbManager.ViewModel;
using SiaqodbManager.Util;

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
		EncryptionViewModelAdapter viewModel;

		//strongly typed window accessor
		public new EncryptionWindow Window {
			get {
				return (EncryptionWindow)base.Window;
			}
		}

		partial void OnOk (NSObject sender)
		{ 
			var pasCont = new PasswordContainer(PasswordText.StringValue);
			viewModel.EncryptCommand(pasCont);
			this.Close();
		}

		partial void OnCancel(NSObject sender){
			this.Close();
		}

		public override void AwakeFromNib ()
		{
			viewModel = new EncryptionViewModelAdapter (EncryptionViewModel.Instance);

			AlgorithmCombo.Bind ("value",viewModel,"Algorithm",BindingUtil.ContinuouslyUpdatesValue);

			EncryptionCheck.Bind ("value",viewModel,"IsEncryptionChecked",BindingUtil.ContinuouslyUpdatesValue);

			AlgorithmCombo.Bind ("enabled",viewModel,"IsEncryptionChecked",null);
			PasswordText.Bind ("enabled",viewModel,"IsEncryptionChecked",null);
		}

		public override void Close ()
		{
			base.Close ();
			NSApplication.SharedApplication.AbortModal ();
		}
	}
}

