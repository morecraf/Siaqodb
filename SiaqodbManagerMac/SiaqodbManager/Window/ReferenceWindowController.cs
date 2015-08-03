
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using SiaqodbManager.DataSourcesAdapters;
using SiaqodbManager.ViewModel;
using MonoMac.ObjCRuntime;
using SiaqodbManager.CustomWindow;
using SiaqodbManager.Util;

namespace SiaqodbManager
{
	public partial class ReferenceWindowController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors

		// Called when created from unmanaged code
		public ReferenceWindowController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public ReferenceWindowController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Call to load from the XIB/NIB file
		public ReferenceWindowController () : base ("ReferenceWindow")
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
		}

		#endregion

		//strongly typed window accessor
		public new ReferenceWindow Window {
			get {
				return (ReferenceWindow)base.Window;
			}
		}

		ReferencesViewModelAdapter referenceViewModel;

		partial void OnCancel (NSObject sender)
		{
			Close();
		}

		void BindButton (NSButton button,NSObject target,string selector)
		{
			button.Target = target;
			button.Action = new Selector (selector);
		}

		public override void AwakeFromNib ()
		{
			var viewModel = new ReferencesViewModel (new OpenFileService ("dll", "Add reference"));
			viewModel.ClosingRequest += CloseWindow;
			referenceViewModel = new ReferencesViewModelAdapter (viewModel);

			ReferencesArray.Bind ("contentArray",referenceViewModel,"References",null);
			Namespaces.Bind ("value",referenceViewModel,"Namespaces",BindingUtil.ContinuouslyUpdatesValue);

			BindButton (AddDefaultButton,referenceViewModel,"AddDefaultCommand");
			BindButton (AddButton,referenceViewModel,"AddCommand");
			BindButton (RemoveButton,referenceViewModel,"RemoveCommand");
			BindButton (OkButton,referenceViewModel,"LoadReferencesCommand");

			ReferencesArray.AddObserver (referenceViewModel, new NSString ("selectionIndexes"), NSKeyValueObservingOptions.New, IntPtr.Zero);
		}

		void CloseWindow (object sender, EventArgs e)
		{
			Close ();
		}

		public override void Close ()
		{
			base.Close ();
			NSApplication.SharedApplication.AbortModal ();
		}
	}
}

