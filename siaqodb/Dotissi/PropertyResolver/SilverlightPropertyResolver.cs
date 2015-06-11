using System;
using System.Reflection;
using Sqo.Meta;
using Sqo;
namespace Dotissi.PropertyResolver
{
    class SilverlightPropertyResolver
    {
        public static string GetPrivateFieldName(PropertyInfo pi, Type ti)
        {
            string backingField = "<" + pi.Name + ">";
            FieldInfo[] fields=ti.GetFields(BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
            foreach (FieldInfo fi in fields)
            {
                if (fi.Name.StartsWith(backingField))
                {
                    return fi.Name;
                }
            }
            
            return null;
           
        }
    }
}
