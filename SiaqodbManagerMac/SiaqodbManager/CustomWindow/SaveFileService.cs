using System;

using MonoMac.AppKit;
using SiaqodbManager.MacWinInterface;

namespace SiaqodbManager.CustomWindow
{
	public class SaveFileService:IDialogService
	{
		public SaveFileService ()
		{
		}

		#region IDialogService implementation

		public string OpenDialog ()
		{
			var savePanel = new NSSavePanel();
			savePanel.ReleasedWhenClosed = true;
			savePanel.Prompt = "Save";
			savePanel.CanCreateDirectories = true;

			var result = savePanel.RunModal();
			if (result == 1)
			{
				return savePanel.Url.Path;
			}
			return "";
		}

		#endregion
	}
}

