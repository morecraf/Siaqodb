using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbCloud
{
    public class BucketNotFoundException : Exception
    {
        public BucketNotFoundException(string message) : base(message)
        {

        }
    }
    public class InvalidVersionFormatException : Exception
    {
        public InvalidVersionFormatException(string message) : base(message)
        {

        }
    }
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message)
        {

        }
    }
}
