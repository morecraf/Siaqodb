using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptonor.Exceptions
{
    public class CryptonorException:Exception
    {
        public CryptonorException():base()
		{

		}
        public CryptonorException(string message): base(message)
		{

		}
    }
}
