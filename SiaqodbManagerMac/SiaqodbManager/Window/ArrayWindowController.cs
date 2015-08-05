
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using System.Text;
using SiaqodbManager.Util;

namespace SiaqodbManager
{
	public partial class ArrayWindowController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors

		// Called when created from unmanaged code
		public ArrayWindowController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public ArrayWindowController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Call to load from the XIB/NIB file
		public ArrayWindowController () : base ("ArrayWindow")
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
		}

		private string array;

		public Array Values{
			get{ return array.TrimEnd().Split ('\n').ToArray ();}
		}

		public ArrayWindowController (Array values) : base ("ArrayWindow")
		{
			var builder = new StringBuilder ();
			foreach(var value in values){
				builder.Append (value.ToString());
				builder.Append ("\n");
			}
			array = builder.ToString ();
		}
		#endregion

		//strongly typed window accessor
		public new ArrayWindow Window {
			get {
				return (ArrayWindow)base.Window;
			}
		}

		public bool HasValue{ get; private set;}

		public override void AwakeFromNib ()
		{
		//	ArrayText.StringValue = array;
			ArrayText.Bind ("value",this,"Array",BindingUtil.ContinuouslyUpdatesValue);
		}

		[Export("Array")]
		public string Array{
			get{
				return array;
			}
			set{
				array = value;
			}
		}

		partial void OnSave (NSObject sender)
		{
			HasValue = true;
			Close();
		}

		partial void OnCancel (NSObject sender)
		{
			HasValue = false;
			Close();
		}

		public override void Close ()
		{
			base.Close ();
			NSApplication.SharedApplication.AbortModal ();
		}
	}
}

