using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptonorClient.Exceptions
{
    public class WriteException : Exception
    {
        public WriteException()
            : base()
        {

        }
        public WriteException(string message)
            : base(message)
        {

        }
    }
}
