using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqo.Attributes
{
    /// <summary>
    /// Attribute to be used for a member of type String of a storable class to limit Length of a string object to be stored in database 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MaxLengthAttribute:Attribute
    {
        public int maxLength;
        /// <summary>
        /// Create an attribute instance of Type MaxLength
        /// </summary>
        /// <param name="maxLength">number of characters from string to be stored in database</param>
        public MaxLengthAttribute(int maxLength)
        {
           this.maxLength = maxLength;
        }
    }
}
