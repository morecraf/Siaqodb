using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Dotissi
{
    static class MemberInfoExtensions
    {
        public static object[] GetCustomAttributes(this MemberInfo memberInfo, Type attributeType, bool inherit)
        {
            Attribute at= memberInfo.GetCustomAttribute(attributeType, inherit);
            if (at != null)
            {
                return new object[] { at };
            }
            return new object[]{};
        }
        public static MemberTypes GetMemberType(this MemberInfo member)
        {
            if (member is FieldInfo)
                return MemberTypes.Field;
            if (member is ConstructorInfo)
                return MemberTypes.Constructor;
            if (member is PropertyInfo)
                return MemberTypes.Property;
            if (member is EventInfo)
                return MemberTypes.Event;
            if (member is MethodInfo)
                return MemberTypes.Method;

           
            return MemberTypes.TypeInfo;
        } 

    }
    public enum MemberTypes
    { 
        Constructor=1,
        Event=2,
        Field=4,
        Method=8,
        Property=16,
        TypeInfo=32,
        Custom=64,
        NestedType=128,
        All=191
    }
}
