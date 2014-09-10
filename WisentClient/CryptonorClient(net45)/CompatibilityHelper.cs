using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Reflection;


namespace CryptonorClient
{
    static class  CompatibilityHelper
    {
        public static IEnumerable<KeyValuePair<string, string>> ParseQueryString(Uri uri)
        {
            var value = uri.ParseQueryString();             
#if NET
           
            return NameValueToEnumerable(value);
#else 
            return value;
#endif
        }
         #if NET
        private static IEnumerable<KeyValuePair<string, string>> NameValueToEnumerable(System.Collections.Specialized.NameValueCollection nameValueCollection)
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            if (!nameValueCollection.AllKeys.Any())
                return list;

            foreach (var key in nameValueCollection.AllKeys)
            {
                var value = nameValueCollection[key];
                var pair = new KeyValuePair<string, string>(key, value);

                list.Add(pair);
            }
            return list;
        }
#endif

       
    }
#if WinRT
    class Path
    {
        public static char DirectorySeparatorChar { get { return '\\'; } }

        internal static string GetDirectoryName(string fullPath)
        {
            return fullPath.Remove(fullPath.LastIndexOf('\\'));

        }
        internal static string GetFileName(string fullPath)
        {
            return fullPath.Substring(fullPath.LastIndexOf('\\') + 1);
        }
    }
#endif
    static class TypeExtensions
    {
#if WinRT
        public static bool IsAssignableFrom(this Type type, Type fromType)
        {
            return type.GetTypeInfo().IsAssignableFrom(fromType.GetTypeInfo());
        }
        public static Type GetInterface(this Type type, string name, bool ignoreCase)
        {

            List<Type> iTypes = type.GetTypeInfo().ImplementedInterfaces.ToList();
            if (iTypes != null)
            {
                foreach (Type t in iTypes)
                {
                    if (name == t.Name)
                    {
                        return t;
                    }
                }
            }

            return null;
        }
        public static bool IsPrimitive(this Type type)
        {
            return type.GetTypeInfo().IsPrimitive;
        }
        public static bool IsClass(this Type type)
        {
            return type.GetTypeInfo().IsClass;
        }
        public static PropertyInfo GetProperty(this Type type, string name)
        {
            PropertyInfo pi = type.GetTypeInfo().GetDeclaredProperty(name);
            if (pi == null)
            {
                if (type.GetTypeInfo().BaseType != null)
                {
                    return GetProperty(type.GetTypeInfo().BaseType, name);
                }
            }
            else return pi;

            return null;
        }
        public static PropertyInfo GetProperty(this Type type, string name, BindingFlags flags)
        {
            return type.GetTypeInfo().GetDeclaredProperty(name);
        }
        public static PropertyInfo[] GetProperties(this Type type)
        {
            return type.GetTypeInfo().DeclaredProperties.ToArray<PropertyInfo>(); 
        }
        public static MethodInfo GetMethod(this Type type, string name)
        {
            return type.GetTypeInfo().GetDeclaredMethod(name);
        }
        public static FieldInfo[] GetFields(this Type type, BindingFlags flags)
        {
            return type.GetTypeInfo().DeclaredFields.ToArray<FieldInfo>();
        }
        public static Type GetBaseType(this Type type)
        {
            return type.GetTypeInfo().BaseType;
        }
        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments;
        }
        public static ConstructorInfo GetConstructor(this Type type, Type[] types)
        {

            foreach (ConstructorInfo ctor in type.GetTypeInfo().DeclaredConstructors)
            {
                ParameterInfo[] prinfos = ctor.GetParameters();
                if (prinfos.Length == types.Length)
                {
                    int ok = 0;
                    for (int i = 0; i < prinfos.Length; i++)
                    {
                        if (prinfos[i].ParameterType == types[i])
                        {
                            ok++;
                        }

                    }
                    if (ok == types.Length)
                    {
                        return ctor;
                    }
                }
            }
            return null;
        }
        public static bool IsSubclassOf(this Type type, Type t)
        {
            return t.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }
        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }
#else
        public static bool IsGenericType(this Type type)
        {
            return type.IsGenericType;
        }
        public static bool IsEnum(this Type type)
        {
            return type.IsEnum;
        }
        public static bool IsClass(this Type type)
        {
            return type.IsClass;
        }
        public static bool IsPrimitive(this Type type  )
        {
            return type.IsPrimitive;
        }
#endif
    }
#if WinRT
    public enum BindingFlags
    {
        Instance = 4,
        Static = 8,
        Public = 16,
        NonPublic = 32,
        FlattenHierarchy = 64
    }
#endif
}
