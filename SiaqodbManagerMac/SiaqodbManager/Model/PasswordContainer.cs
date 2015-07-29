using System;
using SiaqodbManager.MacWinInterface;

namespace SiaqodbManager
{
	public class PasswordContainer:IPasswordContainer
	{
		public PasswordContainer (string password)
		{
			this.Password = password;
		}

		#region IPasswordContainer implementation

		public string Password {
			get;
			set ;
		}

		#endregion
	}
}

