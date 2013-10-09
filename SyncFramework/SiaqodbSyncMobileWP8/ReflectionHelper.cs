using Sqo.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Sqo.Utilities;

namespace SiaqodbSyncMobile
{
    class ReflectionHelper
    {
        //TODO: cache for a Type
        public static int GetIdValue(object obj)
        {
            PropertyInfo pi = GetIdProperty(obj.GetType());
#if UNITY3D
            return (int)pi.GetGetMethod().Invoke(obj, null);
#else

            return (int)pi.GetValue(obj, null);
#endif

        }
        public static string GetIdBackingField(Type type)
        {
            PropertyInfo pi = GetIdProperty(type);
            return ExternalMetaHelper.GetBackingField(pi);
        }
        public static PropertyInfo GetIdProperty(Type type)
        {
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (PropertyInfo pi in pinfos)
            {
                object[] customAttStr = pi.GetCustomAttributes(typeof(JsonProperty), false);
                if (customAttStr.Length > 0)
                {
                    JsonProperty dm = customAttStr[0] as JsonProperty;
                    if (string.Compare(dm.PropertyName, "Id", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        return pi;
                    }
                }
            }
            PropertyInfo piId = type.GetProperty("Id", flags);
            if (piId == null)
            {
                piId = type.GetProperty("id", flags);
                if (piId == null)
                {
                    piId = type.GetProperty("ID", flags);
                }
            }
            if (piId != null)
            {
                return piId;
            }
            throw new SiaqodbException("Type of object not have Id property required by Azure Mobile Services");
        }
    }
}
