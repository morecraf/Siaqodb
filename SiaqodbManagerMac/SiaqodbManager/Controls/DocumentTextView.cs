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
			AutoresizingMask = NSViewResizingMask.HeightSizable 
			  | NSViewResizingMask.WidthSizable|
				NSViewResizingMask.MaxXMargin |
				NSViewResizingMask.MaxYMargin;

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
				@"(¤(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\¤])*¤(\s*:)?|\b(from|where|select|
							group|into|orderby|join|let|in|on|equals|descending|
							ascending|by|null|abstract|add|as|ascending|
							async|await|base|bool|break|by|byte|case|catch|char|checked|class|
							const|continue|decimal|default|delegate|descending|do|double|
							dynamic|else|enum|equals|explicit|extern|false|finally|fixed|float|for|foreach|
							from|get|global|goto|group|if|implicit|in|int|interface|internal|into|
							is|join|let|lock|long|namespace|new|null|object|on|operator|orderby|
							out|override|params|partial|private|protected|public|readonly|
							ref|remove|return|sbyte|sealed|select|set|short|sizeof|stackalloc|static|string|
							struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|
							using|value|var|virtual|void|volatile|where|while|
							yield)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)".Replace('¤', '"'),
				match => {
					if (Regex.IsMatch(match.Value, @"^¤".Replace('¤', '"'))) {
						if (Regex.IsMatch(match.Value, ":$")) {
							SetTextColor(NSColor.Blue,new NSRange(match.Index,match.Length));
							return match.Value;
						} else {
							SetTextColor(NSColor.Blue,new NSRange(match.Index,match.Length));
							return match.Value;
						}
					} else if (Regex.IsMatch(match.Value, "from|where|select|group|into|orderby|join|let|in|on|equals|descending|ascending|by")) {
						SetTextColor(NSColor.Blue,new NSRange(match.Index,match.Length));
						return match.Value;
					}else if (Regex.IsMatch(match.Value, "byte|ushort|decimal|bool|char|string|double|float|int|void|uint|sbyte|ulong|long")) {
						SetTextColor(NSColor.Blue,new NSRange(match.Index,match.Length));
						return match.Value;
					}else if (Regex.IsMatch(match.Value, "true|false|abstract|add|as|ascending|async|await|base" +
						"|break|by|case|catch|checked|class|const|continue|default|delegate|descending|do|" +
						"dynamic|else|enum|equals|texplicit|extern|false|finally|fixed|for|foreach|from|get|global|goto|" +
						"group|if|implicit|in|interface|internal|into|is|join|let|lock|namespace|new|null|" +
						"object|on|operator|orderby|out|override|params|partial|private|protected|public|readonly|" +
						"ref|remove|return|tsealed|select|set|short|sizeof|stackalloc|static|struct|switch|this|" +
						"throw|true|try|typeof|unchecked|unsafe|using|value|var|virtual|volatile|where|while|yield")) {
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

