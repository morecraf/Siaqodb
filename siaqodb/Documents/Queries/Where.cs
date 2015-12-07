using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sqo.Documents
{
    [System.Reflection.Obfuscation(Exclude = true)]
    internal class Where 
    {
        public Where(string tagOrKey)
        {
            this.TagName = tagOrKey;

        }
        public WhereOp Operator { get; set; }
        public string TagName { get; set; }
        public object Value { get; set; }
        public int? Skip { get; set; }
        public int? Limit { get; set; }
        public bool? Descending { get; set; }
        public object[] In { get; set; }
        public object[] Between { get; set; }
        
    }
    internal enum WhereOp { Equal, NotEqual, LessThan, LessThanOrEqual, GreaterThan, GreaterThanOrEqual, StartWith, EndWith, Contains,In, Between, BetweenExceptStart, BetweenExceptEnd, BetweenExceptStartEnd }
}
