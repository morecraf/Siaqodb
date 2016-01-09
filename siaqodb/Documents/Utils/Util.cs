using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Sqo.Documents.Utils
{
    class Util
    {

        public static object ChangeType(object obj, Type t)
        {
#if SILVERLIGHT
                        return Convert.ChangeType(obj, t, System.Threading.Thread.CurrentThread.CurrentCulture);
#elif  CF
                           return      Convert.ChangeType(obj, t,System.Globalization.CultureInfo.CurrentCulture);
#else
            return Convert.ChangeType(obj, t);
#endif
        }
        public static int Compare(object a, object b)
        {
            int c = 0;
            if (a == null || b == null)
            {
                if (a == b)
                    c = 0;
                else if (a == null)
                    c = -1;
                else if (b == null)
                    c = 1;
            }
            else
            {
                if (b.GetType() != a.GetType())
                {
                    b = Util.ChangeType(b, a.GetType());
                }
                c = ((IComparable)a).CompareTo(b);
            }
            return c;
        }
        public static bool Is64BitOperatingSystem()
        {
            return IntPtr.Size == 8;
        }

    }
}
