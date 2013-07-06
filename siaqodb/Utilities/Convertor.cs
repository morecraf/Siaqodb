using System;


namespace Sqo.Utilities
{
    internal class Convertor
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
    }
}
