using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dotissi

{
    class Path
    {
        public static char DirectorySeparatorChar { get { return '\\';} }

        internal static string GetDirectoryName(string fullPath)
        {
            return fullPath.Remove(fullPath.LastIndexOf('\\'));
           
        }
        internal static string GetFileName(string fullPath)
        {
            return fullPath.Substring(fullPath.LastIndexOf('\\') + 1);
        }
    }
}
