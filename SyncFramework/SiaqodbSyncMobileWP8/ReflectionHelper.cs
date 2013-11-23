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
        private static Dictionary<Type,string> idBackingFields=new Dictionary<Type,string>();
        public static string GetIdBackingField(Type type)
        {
            if (!idBackingFields.ContainsKey(type))
            {
                PropertyInfo pi = GetIdProperty(type);
                idBackingFields[type] = ExternalMetaHelper.GetBackingField(pi);
            }
            return idBackingFields[type];
        }
        public static PropertyInfo GetIdProperty(Type type)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (PropertyInfo pi in pinfos)
            {
                object[] customAttStr = pi.GetCustomAttributes(typeof(JsonProperty), false);
                if (customAttStr.Length > 0)
                {
                    JsonProperty dm = customAttStr[0] as JsonProperty;
                    if (string.Compare(dm.PropertyName, "Id", StringComparison.OrdinalIgnoreCase) == 0)
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
            throw new SiaqodbException("Type of object does not have Id property required by Azure Mobile Services");
        }
       
        public static string GetDiscoveringTypeName(Type type)
        {

            string onlyTypeName = type.Namespace + "." + type.Name;

#if SILVERLIGHT
            string assemblyName = type.Assembly.FullName.Split(',')[0];
#elif NETFX_CORE
            string assemblyName = type.GetTypeInfo().Assembly.GetName().Name;
#else
           string assemblyName = type.Assembly.GetName().Name;
#endif

            string[] tNames = new string[] { onlyTypeName, assemblyName };

            return tNames[0] + ", " + tNames[1];

        }
        public static Type GetTypeByDiscoveringName(string typeName)
        {
            #if SILVERLIGHT
            typeName  += ", Version=0.0.0.1,Culture=neutral, PublicKeyToken=null";
            #endif
            return Type.GetType(typeName);
        }
    }
    static class TypeExtensions
    {
#if NETFX_CORE
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
        public static bool IsPrimitive(this Type type  )
        {
            return type.GetTypeInfo().IsPrimitive;
        }
        public static bool IsClass(this Type type)
        {
            return type.GetTypeInfo().IsClass;
        }
        public static PropertyInfo GetProperty(this Type type, string name)
        {
            PropertyInfo pi= type.GetTypeInfo().GetDeclaredProperty(name);
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
        public static PropertyInfo GetProperty(this Type type, string name,BindingFlags flags)
        {
            return type.GetTypeInfo().GetDeclaredProperty(name);
        }
        public static PropertyInfo[] GetProperties(this Type type,BindingFlags flags)
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
                ParameterInfo[] prinfos= ctor.GetParameters();
                if (prinfos.Length == types.Length)
                {
                    int ok=0;
                    for (int i = 0; i < prinfos.Length;i++ )
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
        public static object[] GetCustomAttributes(this MemberInfo memberInfo, Type attributeType, bool inherit)
        {
            Attribute at = memberInfo.GetCustomAttribute(attributeType, inherit);
            if (at != null)
            {
                return new object[] { at };
            }
            return new object[] { };
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
        public static bool IsPrimitive(this Type type)
        {
            return type.IsPrimitive;
        }
#endif
    }
#if NETFX_CORE
    public enum BindingFlags
    { 
        Instance=4,
        Static=8,
        Public=16,
        NonPublic=32,
        FlattenHierarchy=64
    }
#endif
}
