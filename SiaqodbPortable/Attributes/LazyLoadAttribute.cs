using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqo.Attributes
{
    /// <summary>
    /// The property/field will not be loaded by default, it will be loaded by using Include(...) method
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class LazyLoadAttribute : System.Attribute
    {
        public LazyLoadAttribute()
        {

        }

    }
}
