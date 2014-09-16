using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if ASYNC
using System.Threading.Tasks;
#endif

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
