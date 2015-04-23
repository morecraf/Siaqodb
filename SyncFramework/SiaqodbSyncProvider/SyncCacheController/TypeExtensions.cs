using Sqo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Synchronization.Services.Formatters
{

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
        public static IEnumerable<PropertyInfo> GetProperties(this Type type)
        {
            return type.GetTypeInfo().DeclaredProperties;
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
}
