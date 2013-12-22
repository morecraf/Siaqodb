using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Sqo.Attributes
{
    /// <summary>
    /// Make property to be stored as a Document-a snapshot of current object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DocumentAttribute : System.Attribute
    {
        public DocumentAttribute()
        {

        }

    }
}
