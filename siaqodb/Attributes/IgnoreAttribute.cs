using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqo.Attributes
{
    /// <summary>
    /// Attribute to be used for a member of a storable class and that object will be ignored by siaqodb engine
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class IgnoreAttribute:System.Attribute
    {
        public IgnoreAttribute()
        {

        }
        
    }
}
