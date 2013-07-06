using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqo.Exceptions
{
   
    public class UniqueConstraintException : Exception
    {
        public UniqueConstraintException()
            : base()
        {

        }
        public UniqueConstraintException(string message)
            : base(message)
        {

        }
    }
}
