using System;
using SiaqodbManager.MacWinInterface;
using MonoMac.AppKit;

namespace SiaqodbManager.CustomWindow
{
	public class OpenFileService:IDialogService
	{
		private string extension;
		public OpenFileService (string extension)
		{
			this.extension = extension;
		}

		#region IDialogService implementation

		public string OpenDialog ()
		{
			var referencePanel = new NSOpenPanel();
			referencePanel.ReleasedWhenClosed = true;
			referencePanel.Prompt = "Add reference";
			referencePanel.CanCreateDirectories = true;
			referencePanel.AllowedFileTypes = new [] {extension};

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

