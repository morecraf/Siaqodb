using System;
using SiaqodbManager.MacWinInterface;
using MonoMac.AppKit;

namespace SiaqodbManager.CustomWindow
{
	public class OpenFolderService:IDialogService
	{
		string message;
		public OpenFolderService(string message){
			this.message = message;
		}

		#region IDialogService implementation

		public string OpenDialog ()
		{
			var openPanel = new NSOpenPanel();
			openPanel.ReleasedWhenClosed = true;
			openPanel.Prompt = message;
			openPanel.CanCreateDirectories = true;
			openPanel.CanChooseDirectories = true;
			openPanel.CanChooseFiles = false;

			var result = openPanel.RunModal();
			if (result == 1)
			{
				return openPanel.Url.Path;
			}
			return "";
		}

		#endregion

	
	}
}

