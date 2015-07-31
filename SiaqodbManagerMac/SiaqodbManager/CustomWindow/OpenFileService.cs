using System;
using SiaqodbManager.MacWinInterface;
using MonoMac.AppKit;

namespace SiaqodbManager.CustomWindow
{
	public class OpenFileService:IDialogService
	{
		private string extension;
		private string message;

		public OpenFileService (string extension, string message)
		{
			this.extension = extension;
			this.message = message;
		}

		#region IDialogService implementation

		public string OpenDialog ()
		{
			var referencePanel = new NSOpenPanel();
			referencePanel.ReleasedWhenClosed = true;
			referencePanel.Prompt = message;
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

