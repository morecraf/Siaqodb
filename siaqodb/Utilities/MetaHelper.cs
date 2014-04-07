using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Meta;
using System.Reflection;
using Sqo.Exceptions;
using Sqo.Attributes;
using System.IO;
using Sqo;
namespace Sqo.Utilities
{
    class MetaHelper
    {

        public static string GetBackingFieldByAttribute(MemberInfo mi)
        { 
             object[] customAttStr = mi.GetCustomAttributes(typeof(UseVariableAttribute), false);
             if (customAttStr.Length > 0)
             {
                 UseVariableAttribute uv = customAttStr[0] as UseVariableAttribute;
                 return uv.variableName;
             }
             if (SiaqodbConfigurator.PropertyMaps != null)
             {
                 if (SiaqodbConfigurator.PropertyMaps.ContainsKey(mi.DeclaringType))
                 {
                     if (SiaqodbConfigurator.PropertyMaps[mi.DeclaringType].ContainsKey(mi.Name))
                     {
                         return SiaqodbConfigurator.PropertyMaps[mi.DeclaringType][mi.Name];
                     }
                 }
             }
             return null;
        }
        public static FieldSqoInfo FindField(List<FieldSqoInfo> list, string fieldName)
        {
            foreach (FieldSqoInfo fi in list)
            {
                if (string.Compare(fi.Name, fieldName) == 0)
                {
                    return fi;
                }
            }
            return null;
        }
        public static PropertyInfo GetAutomaticProperty(FieldInfo field)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

            if (field.Name.StartsWith("<") && field.Name.Contains(">"))
            {
                PropertyInfo pi = field.DeclaringType.GetProperty(field.Name.Substring(1, field.Name.IndexOf('>') - 1), flags);
                return pi;
            }
            return null;
        }
       
        
#if SILVERLIGHT        
        public static void CallSetValue(FieldInfo fi, object fieldValue, object obj, Type ti)
        {

            ISqoDataObject dataObje = obj as ISqoDataObject;
            if (dataObje != null)
            {
                dataObje.SetValue(fi, fieldValue);
            }
            else//set oid by reflection
            {
                try
                {
                    var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;

                    MethodInfo mi = ti.GetMethod("SetValue", flags);
                    if (mi == null)
                    {
                        throw new Exception("Object not have method SetValue, define it first!");
                    }
                    else
                    {
                        mi.Invoke(obj, new object[] { fi, fieldValue });
                    }
                }
                catch
                {
                    throw new SiaqodbException("Override GetValue and SetValue methods of SqoDataObject-Silverlight limitation to private fields");
                }
            }


        }
        public static object CallGetValue(FieldInfo fi, object obj, Type ti)
        {
            
 

            ISqoDataObject dataObje = obj as ISqoDataObject;
            if (dataObje != null)
            {
                return dataObje.GetValue(fi);
            }
            else//set oid by reflection
            {
                var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;

                MethodInfo mi = ti.GetMethod("GetValue", flags);
                if (mi == null)
                {
                    throw new Exception("Object not have method GetValue, define it first!");
                }
                else
                {
                    return mi.Invoke(obj, new object[] { fi });
                }
            }


        }
   
#endif
        internal static ulong GetTickCountOfObject(object obj, Type ti)
        {
            FieldInfo fi = MetaExtractor.FindField(ti, "tickCount");
            if (fi != null)
            {
                if (fi.FieldType == typeof(ulong))
                {
                    #if SILVERLIGHT
                    return (ulong)CallGetValue(fi, obj, ti);
                    #else
                        return (ulong)fi.GetValue(obj);
                    #endif
                }
            }

            return 0;

        }
       
        internal static int PaddingSize(int length)
        {
            if (SiaqodbConfigurator.EncryptedDatabase)
            {
                int blockSize = SiaqodbConfigurator.Encryptor.GetBlockSize() / 8;
                if (length % blockSize == 0) //for enncryption for 64 bits size( 8 bytes)
                    return length;
                else
                    return length + (blockSize - (length % blockSize));
            }
            else return length;
        }
        internal static int GetLengthOfType(Type ty)
        {
            int typeId = MetaExtractor.GetAttributeType(ty);

            return MetaExtractor.GetAbsoluteSizeOfField(typeId);
        }
        internal static long GetSeekPosition(SqoTypeInfo ti, int oid)
        {
            long position = (long)ti.Header.headerSize + (long)((long)(oid - 1) * (long)ti.Header.lengthOfRecord);
            return position;
        }
        internal static string GetFieldAsInDB(string field, Type type)
        {
            List<FieldInfo> fi = new List<FieldInfo>();
            Dictionary<FieldInfo, PropertyInfo> automaticProperties = new Dictionary<FieldInfo, PropertyInfo>();
            MetaExtractor.FindFields(fi, automaticProperties, type);
            
            foreach (FieldInfo f in fi)
            {
                if (f.Name == field)
                {
                    return f.Name;
                }
                else if (automaticProperties.ContainsKey(f))
                {
                    if (field == automaticProperties[f].Name)
                    {
                        return f.Name;
                    }
                }
            }

            throw new SiaqodbException("Field:" + field + " not found as field or as automatic property of Type provided");

        }
#if !WinRT
        internal static bool FileExists(string dbpath, string typeName, bool useElevatedTrust)
        {
            string extension = ".sqo";
            if (SiaqodbConfigurator.EncryptedDatabase)
            {
                extension = ".esqo";
            }
            string fileName = dbpath + Path.DirectorySeparatorChar + typeName + extension;
            #if SILVERLIGHT
                if (!useElevatedTrust)
                {
                    System.IO.IsolatedStorage.IsolatedStorageFile isf = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();

                    return isf.FileExists(fileName);
                    
                }
                else
                {
                    return File.Exists(fileName);
                    
                }
				

#elif MONODROID
			return File.Exists(fileName);
               
               
#else
            return File.Exists(fileName);
               
           

#endif
            
        }
#endif
        internal static string GetOldFileNameByType(Type type)
        {
            
            string[] tNames = type.AssemblyQualifiedName.Split(',');

#if SILVERLIGHT
			string tName = type.AssemblyQualifiedName;
#else

            string tName = tNames[0] + "," + tNames[1];
#endif

            string[] split = tName.Split(',');
            string fileName = split[0] + "." + split[1];

#if SILVERLIGHT
            if (!SiaqodbConfigurator.UseLongDBFileNames)
            {
                fileName = fileName.GetHashCode().ToString();
            }
#endif

            return fileName;
        }
#if !WinRT
        internal static object GetFieldValue(object obj, Type type, string fieldName)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
             FieldInfo fi= type.GetField(fieldName,flags);
             if (fi != null)
             {
#if SILVERLIGHT
                 return CallGetValue(fi, obj, type);
#else
                 return fi.GetValue(obj);
#endif
             }
             return null;
        }
#endif
        internal static bool TypeHasOID(Type t)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public;

            PropertyInfo pi = t.GetProperty("OID", flags);
            return pi != null;
            
        }
        internal static string GetDiscoveringTypeName(Type type)
        {

            string onlyTypeName = type.Namespace + "." + type.Name;

#if SILVERLIGHT
            string assemblyName = type.Assembly.FullName.Split(',')[0];
#elif NETFX_CORE
            string assemblyName = type.GetTypeInfo().Assembly.GetName().Name;
#else
            string assemblyName = type.Assembly.GetName().Name;
#endif

            return onlyTypeName + ", " + assemblyName;

        }
        internal static Type GetTypeByDiscoveringName(string typeName)
        {
#if SILVERLIGHT
            typeName  += ", Version=0.0.0.1,Culture=neutral, PublicKeyToken=null";
#endif
            return Type.GetType(typeName);
        }
        
    }
}
