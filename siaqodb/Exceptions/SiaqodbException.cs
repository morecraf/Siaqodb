using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqo.Exceptions
{
	public class SiaqodbException:Exception
	{

		public SiaqodbException():base()
		{

		}
		public SiaqodbException(string message): base(message)
		{

		}
	}
}
