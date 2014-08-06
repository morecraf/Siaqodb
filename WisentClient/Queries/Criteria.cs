using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if ASYNC
using System.Threading.Tasks;
#endif
namespace CryptonorClient
{
    public class Criteria
    {
        public const string Equal = "eq";
        public const string LessThan = "lt";
        public const string LessThanOrEqual = "le";
        public const string GreaterThan = "gt";
        public const string GreaterThanOrEqual = "ge";
        public string TagName { get; set; }
        public object TagValue { get; set; }
        public string OperationType { get; set; }
        public string TagType { get; set; }
    }
    
}
