using System;


namespace Sqo.Exceptions
{
    public class OptimisticConcurrencyException:Exception
    {
        public OptimisticConcurrencyException(): base()
		{

		}
		public OptimisticConcurrencyException(string message):base(message)
		{

		}
    }
}
