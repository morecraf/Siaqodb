using System;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;

namespace SiaqodbManager
{
	public class CellWithError:NSTextFieldCell
	{
		public string Message{ get; set; }

		public CellWithError (string message)
		{
			this.Message = message;
			Editable = true;
		}

		public CellWithError(IntPtr ptr):base(ptr){
		}

		public override void DrawWithFrame (System.Drawing.RectangleF cellFrame, NSView inView)
		{
			base.DrawWithFrame (cellFrame,inView);

			var context = NSGraphicsContext.CurrentContext.GraphicsPort;
			context.SetStrokeColor (new CGColor(255, 0, 0)); // red
			context.SetLineWidth (1.0F);
			context.StrokeRect (cellFrame);
			var customTable = inView as CustomTable;

			if(customTable != null){
				inView.AddToolTip (cellFrame,new NSString(Message),IntPtr.Zero);
			}
		}
	}
}

