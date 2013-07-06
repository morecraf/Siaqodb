using Sqo.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbSyncMobile
{
    class ReflectionHelper
    {
        //TODO: cache for a Type
        public static int GetIdValue(object obj)
        {
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
             PropertyInfo[] pinfos= obj.GetType().GetProperties(flags);
             foreach (PropertyInfo pi in pinfos)
             {
                 object[] customAttStr = pi.GetCustomAttributes(typeof(System.Runtime.Serialization.DataMemberAttribute), false);
                 if (customAttStr.Length > 0)
                 {
                    System.Runtime.Serialization.DataMemberAttribute dm= customAttStr[0] as System.Runtime.Serialization.DataMemberAttribute;
                    if (string.Compare(dm.Name, "Id", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
#if UNITY3D
                    return (int)pi.GetGetMethod().Invoke(obj, null);
#else

                        return (int)pi.GetValue(obj, null);
#endif
                    }

                 }

             }
             PropertyInfo piId = obj.GetType().GetProperty("Id", flags);
             if (piId != null)
             {
#if UNITY3D
                    return (int)piId.GetGetMethod().Invoke(obj, null);
#else

                 return (int)piId.GetValue(obj, null);
#endif
             }
             throw new SiaqodbException("Type of object not have Id property required by Azure Mobile Services");
               
        }
    }
}
