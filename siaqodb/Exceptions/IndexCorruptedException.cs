using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sqo.Exceptions
{
    public class IndexCorruptedException : Exception
    {
        public IndexCorruptedException()
            : base()
        {

        }
        public IndexCorruptedException(string message)
            : base(message)
        {

        }
    }
}
