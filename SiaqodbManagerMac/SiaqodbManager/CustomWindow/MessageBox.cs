using System;
using MonoMac.AppKit;
using SiaqodbManager.MacWinInterface;

namespace SiaqodbManager.CostumWindow
{
	public class MessageBox:IMessageBox
	{


		#region IMessageBox implementation

		public void Show (string message)
		{
			throw new NotImplementedException ();
		}

		public bool Show (string message, string title, bool YesNo)
		{
			var alert = new NSAlert {
				MessageText = message,
				AlertStyle = NSAlertStyle.Informational,
			};

			alert.AddButton("Cancel");
			alert.AddButton ("OK");
			var result =  alert.RunModal();
			return result != 1000;
		}
		#endregion
	}
}

