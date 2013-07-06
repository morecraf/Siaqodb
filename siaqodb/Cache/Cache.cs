using System;
using System.Collections.Generic;
using System.Text;
using Sqo.Meta;
using Sqo.Exceptions;
using System.Collections;

namespace Sqo.Cache
{
	class Cache
	{
        private static Dictionary<Type, int> cacheOfTypesByIds = new Dictionary<Type, int>();
            

         static Cache()
        {
            AddNativeTypes();
        }
       
        public static void AddTypeBytID(Type type, int ID)
        {
            cacheOfTypesByIds[type] =ID ;
        }
       
        public static bool ContainsPrimitiveType(Type type)
        {
            return cacheOfTypesByIds.ContainsKey(type);
        }
       
        public static int GetTypeID(Type t)
        {
            return cacheOfTypesByIds[t];
        }
        public static Type GetTypebyID(int ID)
        {
            if (ID > MetaExtractor.ArrayTypeIDExtra)
            {
                ID -= MetaExtractor.ArrayTypeIDExtra;
            }
            else if (ID < MetaExtractor.ArrayTypeIDExtra && ID > MetaExtractor.FixedArrayTypeId)
            {
                ID -= MetaExtractor.FixedArrayTypeId;
            }
            if (ID == MetaExtractor.textID)//workaround to store Text and String same type string
            {
                return GetTypebyID(MetaExtractor.stringID);
            }
            foreach (Type t in cacheOfTypesByIds.Keys)
            { 
                if(cacheOfTypesByIds[t]==ID)
                {
                    return t;
                }
            }
            throw new SiaqodbException("Unsupported type ID:" + ID.ToString());

        }
        

        private static void AddNativeTypes()
        {
            // Primitive integer types
            cacheOfTypesByIds[typeof(int)] = MetaExtractor.intID;
            cacheOfTypesByIds[typeof(uint)] = MetaExtractor.uintID;
            cacheOfTypesByIds[typeof(short)] = MetaExtractor.shortID;
            cacheOfTypesByIds[typeof(ushort)] = MetaExtractor.ushortID;
            cacheOfTypesByIds[typeof(byte)] = MetaExtractor.byteID;
            cacheOfTypesByIds[typeof(sbyte)] = MetaExtractor.sbyteID;
            cacheOfTypesByIds[typeof(long)] = MetaExtractor.longID;
            cacheOfTypesByIds[typeof(ulong)] = MetaExtractor.ulongID;


            // Primitive decimal types
            cacheOfTypesByIds[typeof(float)] = MetaExtractor.floatID;
            cacheOfTypesByIds[typeof(double)] = MetaExtractor.doubleID;
            cacheOfTypesByIds[typeof(decimal)] =MetaExtractor.decimalID;

            // Char
            cacheOfTypesByIds[typeof(char)] = MetaExtractor.charID;


            // Bool
            cacheOfTypesByIds[typeof(bool)] = MetaExtractor.boolID;



            // Other system value types
            cacheOfTypesByIds[typeof(TimeSpan)] = MetaExtractor.TimeSpanID;
            cacheOfTypesByIds[typeof(DateTime)] = MetaExtractor.DateTimeID;
#if !CF
            cacheOfTypesByIds[typeof(DateTimeOffset)] = MetaExtractor.DateTimeOffsetID;
#endif     
            cacheOfTypesByIds[typeof(Guid)] = MetaExtractor.GuidID;
            cacheOfTypesByIds[typeof(string)] = MetaExtractor.stringID;
            //text                             =24;

            //cacheOfTypesByIds[typeof(Array)] = 30;
           // cacheOfTypesByIds[typeof(IList)] = 31;

        }
		


    }
}
