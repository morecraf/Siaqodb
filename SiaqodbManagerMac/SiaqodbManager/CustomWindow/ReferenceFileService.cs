using System;
using SiaqodbManager.MacWinInterface;
using MonoMac.AppKit;

namespace SiaqodbManager.CustomWindow
{
	public class ReferenceFileService:IDialogService
	{
		public ReferenceFileService ()
		{
		}

		#region IDialogService implementation

		public string OpenDialog ()
		{
			var referencePanel = new NSOpenPanel();
			referencePanel.ReleasedWhenClosed = true;
			referencePanel.Prompt = "Add reference";
			referencePanel.CanCreateDirectories = true;
			referencePanel.AllowedFileTypes = new [] {"dll"};

			var result = referencePanel.RunModal();
			if (result == 1)
			{
				return referencePanel.Url.Path;
			}
			return "";
		}

		#endregion
	}
}

