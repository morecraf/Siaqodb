using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.MetaObjects;
using Sqo.Core;
using System.IO;
using System.Linq.Expressions;
#if ASYNC
using System.Threading.Tasks;
#endif
#if WinRT
using Windows.Storage;
#endif
namespace Sqo
{


    public static class SqoStringExtensions
    {
        /// <summary>
        ///  Returns a value indicating whether the specified System.String object occurs
        ///    within this string.A parameter specifies the type of search
        ///     to use for the specified string.
        /// </summary>
        /// <param name="stringObj">Input string</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="comparisonType"> One of the enumeration values that specifies the rules for the search.</param>
        /// <returns>true if the value parameter occurs within this string, or if value is the
        ///     empty string (""); otherwise, false.</returns>
        public static bool Contains(this string stringObj, string value, StringComparison comparisonType)
        {
            return stringObj.IndexOf(value, comparisonType) != -1;
        }
      
    }
}
