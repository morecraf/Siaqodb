using System;
using MonoMac.AppKit;
using SiaqodbManager.Entities;

namespace SiaqodbManager
{
	public class ErrorTextView:NSTextView
	{
		public ErrorTextView ()
		{
		}

		public void ErrorOccured (object sender, ErrorMessageArgs e)
		{
			base.Value = e.Message;
		}
	}
}

