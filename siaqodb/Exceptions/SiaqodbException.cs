using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqo.Exceptions
{
    /// <summary>
    /// Primary exception class
    /// </summary>
	public class SiaqodbException:Exception
	{
        /// <summary>
        /// Any additional data provided by the exception
        /// </summary>
        public string TransactionName { get; set; }
        public Guid TransactionGuid { get; set; }
        public Exception ChildException { get; set; }

        /// <summary>
        /// Primary exception with no message
        /// </summary>
		public SiaqodbException():base()
		{
        }

        /// <summary>
        /// Primary exception with a message
        /// </summary>
        /// <param name="message"></param>
		public SiaqodbException(string message): base(message)
		{
		}

        /// <summary>
        /// Primary exception with a message and additional data
        /// </summary>
        public SiaqodbException(string message, Exception err, string name, Guid uid) : base(message)
        {
            ChildException = err;
            TransactionName = name;
            TransactionGuid = uid;
        }

        /// <summary>
        /// Primary exception with a message and a child exception data
        /// </summary>
        public SiaqodbException(string message, Exception err) : base(message)
        {
            ChildException = err;
        }
    }
}
