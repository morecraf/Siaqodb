using System;
using MonoMac.AppKit;
using System.Drawing;
using MonoMac.Foundation;
using System.Text.RegularExpressions;

namespace SiaqodbManager.Controls
{
	public class DocumentTextView:NSTextView
	{
		public DocumentTextView ()
		{

			TextDidChange += SyntaxHighlightJson;
			//inputError = new ValidateionResultAdapter (true,"");
			AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable|
				NSViewResizingMask.MaxXMargin | NSViewResizingMask.MaxYMargin ;

		}
//		[Export("error")]
//		public ValidateionResultAdapter HasErrors{ 
//			get{ return inputError;}
//			private set{
//				inputError = value;
//				ShowError ();
//			}
//		}
		public override void Bind (string binding, MonoMac.Foundation.NSObject observable, string keyPath, MonoMac.Foundation.NSDictionary options)
		{
			base.Bind (binding, observable, keyPath, options);

			SyntaxHighlightJson(null,null);
			SetFont (NSFont.FromFontName ("Courier", 16),new NSRange(0,Value.Length));
		}
		public override void DidChangeText ()
		{
			if (Value == null)
				Value = "";
			base.DidChangeText ();
		}

		public void SyntaxHighlightJson(object sender, EventArgs e)
		{   
			Regex.Replace(
				Value,
				@"(¤(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\¤])*¤(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)".Replace('¤', '"'),
				match => {
					if (Regex.IsMatch(match.Value, @"^¤".Replace('¤', '"'))) {
						if (Regex.IsMatch(match.Value, ":$")) {
							SetTextColor(NSColor.Blue,new NSRange(match.Index,match.Length));
							return match.Value;
						} else {
							SetTextColor(NSColor.Blue,new NSRange(match.Index,match.Length));
							return match.Value;
						}
					} else if (Regex.IsMatch(match.Value, "true|false")) {
						SetTextColor(NSColor.Red,new NSRange(match.Index,match.Length));
						return match.Value;
					} else if (Regex.IsMatch(match.Value, "null")) {
						SetTextColor(NSColor.Red,new NSRange(match.Index,match.Length));
						return match.Value;
					}
					SetTextColor(NSColor.Green,new NSRange(match.Index,match.Length));
					return match.Value;
				});
		}

//		void ShowError ()
//		{
//			if (inputError == null)
//				return;
//			if (!inputError.IsValid)
//			{
//				Layer.BorderWidth = 1f;
//				Layer.BorderColor =  NSColor.Red.CGColor;
//				ToolTip = inputError.Message;
//				return;
//			}
//			Layer.BorderColor = NSColor.Black.CGColor;
//			Layer.BorderWidth = 0;
//			ToolTip = "";
//		}
	}
}

