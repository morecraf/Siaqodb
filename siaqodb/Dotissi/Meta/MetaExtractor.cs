using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Dotissi.Meta;
using Dotissi.Core;

using Dotissi.Utilities;
using System.Collections;
using Sqo.Attributes;
using Sqo.Exceptions;
using Sqo;
using Dotissi.Meta;

namespace Dotissi.Meta
{
	class MetaExtractor
	{
		public const int FieldSize = 220;//(sizeof(int) + 200 + sizeof(int) + sizeof(int)+sizeof(int) +sizeof(int)= 220
        public const int ExtraSizeForArray = 9;// (isNull(bool) + oid of array table(int)+ nrElements(int)
		
        //*************************************TypeIDs*************************/
        public const int intID = 1;
        public const int uintID = 2;
        public const int shortID = 3;
        public const int ushortID = 4;
        public const int byteID = 5;
        public const int sbyteID = 6;
        public const int longID = 7;
        public const int ulongID = 8;


        // Primitive decimal types
        public const int floatID = 9;
        public const int doubleID = 10;
        public const int decimalID = 11;

        // Char
        public const int charID = 12;


        // Bool
        public const int boolID = 13;



        // Other system value types
        public const int TimeSpanID = 20;
        public const int DateTimeID = 21;
        public const int GuidID = 22;
        public const int stringID = 23;
        public const int textID = 24;
        public const int DateTimeOffsetID=25;
        //complex type
        public const int complexID=30;

        public const int dictionaryID = 31;

        public const int jaggedArrayID = 32;
        public const int documentID = 33;

        public const int FixedArrayTypeId = 50;

        public const int ArrayTypeIDExtra = 100;
         //*****************************************************************************/  

		public static SqoTypeInfo GetSqoTypeInfo(object o)
		{
            
			Type t = o.GetType();
			return GetSqoTypeInfo(t);
			
		}
        public static SqoTypeInfo GetSqoTypeInfo(Type t)
        {

            List<FieldInfo> fi = new List<FieldInfo>();
            
            Dictionary<FieldInfo, PropertyInfo> automaticProperties = new Dictionary<FieldInfo, PropertyInfo>();

            FindFields(fi,automaticProperties, t);

            SqoTypeInfo ti = new SqoTypeInfo(t);

            int lengthOfRecord =  sizeof(int); //=> +OID size;
            int i = 0;
            int nrOrder = 1;
            for (i = 0; i < fi.Count; i++)
            {
                int maxLength=-1;
                Type fType = fi[i].FieldType;
                bool isText = false;
                if (IsSpecialType(fType) || fi[i].IsLiteral || fi[i].IsInitOnly)
                {
                    continue;
                }

                //TODO:check ignore for properties(automatic properties)
                #region Ignore field

                object[] customAtt= fi[i].GetCustomAttributes(typeof(IgnoreAttribute), false);
                if (customAtt.Length > 0)
                {
                    continue;
                }
                if (automaticProperties.ContainsKey(fi[i]))
                {
                    customAtt = automaticProperties[fi[i]].GetCustomAttributes(typeof(IgnoreAttribute), false);
                    if (customAtt.Length > 0)
                    {
                        continue;
                    }
                }
                //check config
                if (Sqo.SiaqodbConfigurator.Ignored != null)
                {
                    if (Sqo.SiaqodbConfigurator.Ignored.ContainsKey(ti.Type))
                    {
                        if (Sqo.SiaqodbConfigurator.Ignored[ti.Type].Contains(fi[i].Name))
                        {
                            continue;
                        }
                    }
                }

                #endregion

                #region MaxLength for String

                Type elementType = fType.GetElementType();
                if (typeof(IList).IsAssignableFrom(fType))
                {
                    if (elementType==null)
                    {
                        elementType = fType.GetProperty("Item").PropertyType;
                    }
                }
                if (fType == typeof(string) || elementType==typeof(string))
                {
                    //throw new SiaqodbException("String Type must have MaxLengthAttribute set");
						//TODO: set default 100 MaxLength for a string??? activate when automatically schema supported
                    maxLength = 100;
                    bool maxLengthFound = false;
                    object[] customAttStr = fi[i].GetCustomAttributes(typeof(MaxLengthAttribute), false);
                    if (customAttStr.Length > 0)
                    {
                        MaxLengthAttribute m = customAttStr[0] as MaxLengthAttribute;
                        maxLength = m.maxLength;
                        maxLengthFound = true;
                    }
                    else if (automaticProperties.ContainsKey(fi[i]))
                    {
                        customAttStr = automaticProperties[fi[i]].GetCustomAttributes(typeof(MaxLengthAttribute), false);
                        if (customAttStr.Length > 0)
                        {
                            MaxLengthAttribute m = customAttStr[0] as MaxLengthAttribute;
                            maxLength = m.maxLength;
                            maxLengthFound = true;
                        }
                    }
                    if (!maxLengthFound)
                    {
                        //check config
                        if (Sqo.SiaqodbConfigurator.MaxLengths != null)
                        {
                            if (Sqo.SiaqodbConfigurator.MaxLengths.ContainsKey(ti.Type))
                            {
                                if (Sqo.SiaqodbConfigurator.MaxLengths[ti.Type].ContainsKey(fi[i].Name))
                                {
                                    maxLength = Sqo.SiaqodbConfigurator.MaxLengths[ti.Type][fi[i].Name];
                                }
                            }
                        }
                    }
                    //isText for saving dynamic length type
                    object[] customAttStrText = fi[i].GetCustomAttributes(typeof(TextAttribute), false);
                    if (customAttStrText.Length > 0)
                    {
                        isText = true;
                        maxLength = GetSizeOfField(MetaExtractor.byteID);//get byte type size(simulate array byte[])
                    }
                    else if (automaticProperties.ContainsKey(fi[i]))
                    {
                        customAttStrText = automaticProperties[fi[i]].GetCustomAttributes(typeof(TextAttribute), false);
                        if (customAttStrText.Length > 0)
                        {
                            isText = true;
                            maxLength = GetSizeOfField(MetaExtractor.byteID);//get byte type size(simulate array byte[])
                        }
                    }
                    if (!isText)
                    {
                        //check config
                        if (Sqo.SiaqodbConfigurator.Texts != null)
                        {
                            if (Sqo.SiaqodbConfigurator.Texts.ContainsKey(ti.Type))
                            {
                                if (Sqo.SiaqodbConfigurator.Texts[ti.Type].Contains(fi[i].Name))
                                {
                                    isText = true;
                                }
                            }
                        }
                    }
                }
                #endregion
                int fTypeId = -1;
                if (FieldIsDocument(ti,fi[i], automaticProperties))
                {
                    fTypeId = documentID;
                }
                else
                {
                    fTypeId = GetAttributeType(fType);
                }
                if (fTypeId == -1)
                {
                    throw new NotSupportedTypeException(@"Field:" + fi[i].Name + " of class:" + t.Name + " has type:" + fType.Name + @" which is not supported , check documentation about types supported: http://siaqodb.com/?page_id=620 ");
                }
                if (isText)
                {
                    fTypeId = MetaExtractor.textID;
                }
                FieldSqoInfo ai = new FieldSqoInfo(fTypeId, fType);
                ai.Name = fi[i].Name;
                ai.FInfo = fi[i];
                ai.IsText = isText;
                ai.Header.Length = maxLength == -1 ? GetSizeOfField(fTypeId) : MetaHelper.PaddingSize( maxLength);
                if (fType.IsGenericType() && fTypeId != documentID)
                {
                    Type genericTypeDef = fType.GetGenericTypeDefinition();
                    if (genericTypeDef == typeof(Nullable<>))
                    {
                        ai.Header.Length++;//increase Length to be able to store IsNull byte
                    }
                    else if (typeof(IList).IsAssignableFrom(fType))
                    {
                        ai.Header.Length += ExtraSizeForArray;//store OID from rawdata + number of elements
                        ai.AttributeTypeId += MetaExtractor.ArrayTypeIDExtra;
                    }
                }
                else if (fType.IsArray && fTypeId != documentID)
                {
                    if ( ti.Type.IsGenericType() && ti.Type.GetGenericTypeDefinition() == typeof(Indexes.BTreeNode<>) && (ai.Name == "Keys" || ai.Name == "_childrenOIDs"))
                    {
                        ai.AttributeTypeId += MetaExtractor.FixedArrayTypeId;
                        if (ai.Name == "Keys")
                        {
                            ai.Header.Length = ai.Header.Length * Indexes.BTreeNode<int>.KEYS_PER_NODE;
                        }
                        else
                        {
                            ai.Header.Length = ai.Header.Length * Indexes.BTreeNode<int>.CHILDREN_PER_NODE;
                        }


                    }
                    else
                    {
                        ai.Header.Length += ExtraSizeForArray;//store OID from rawdata + number of elements
                        ai.AttributeTypeId += MetaExtractor.ArrayTypeIDExtra;
                    }
                }
                else if (ai.IsText)
                {
                    ai.Header.Length += ExtraSizeForArray; 
                }
                ai.Header.RealLength = maxLength == -1 ? GetAbsoluteSizeOfField(fTypeId) : maxLength;
                ai.Header.PositionInRecord = lengthOfRecord;
                ai.Header.SizeOfName = ByteConverter.SerializeValueType(ai.Name, typeof(string),ti.Header.version).Length;
                ti.Fields.Add(ai);
                lengthOfRecord += ai.Header.Length;
                
                nrOrder++;

                FindAddConstraints(ti, ai);
                FindAddIndexes(ti, ai);

            }

            ti.Header.lengthOfRecord = lengthOfRecord;
            ti.Header.typeNameSize = ByteConverter.SerializeValueType(ti.TypeName, typeof(string), ti.Header.version).Length;
            ti.Header.NrFields = nrOrder - 1;


            ti.Header.headerSize = sizeof(int) + sizeof(int) + sizeof(long) + sizeof(int) + sizeof(int) + sizeof(int) + sizeof(int) + sizeof(int) + sizeof(int) + sizeof(int) + sizeof(int) + sizeof(int)+
                (ti.Header.NrFields * FieldSize) + ti.Header.typeNameSize;

            ti.Header.positionFirstRecord = ti.Header.headerSize;


            //cacheOfTypes[t] = ti;
            return ti;

        }

        private static bool FieldIsDocument(SqoTypeInfo ti, FieldInfo fieldInfo, Dictionary<FieldInfo, PropertyInfo> automaticProperties)
        {

            object[] customAtt = fieldInfo.GetCustomAttributes(typeof(DocumentAttribute), false);
            if (customAtt.Length > 0)
            {
                return true;
            }
            if (automaticProperties.ContainsKey(fieldInfo))
            {
                customAtt = automaticProperties[fieldInfo].GetCustomAttributes(typeof(DocumentAttribute), false);
                if (customAtt.Length > 0)
                {
                    return true;
                }
            }
            //check config
            if (Sqo.SiaqodbConfigurator.Documents != null)
            {
                if (Sqo.SiaqodbConfigurator.Documents.ContainsKey(ti.Type))
                {
                    if (Sqo.SiaqodbConfigurator.Documents[ti.Type].Contains(fieldInfo.Name))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsSpecialType(Type fType)
        {
            if (fType.IsSubclassOf(typeof(Delegate)))
                return true;
            else if (typeof(IList).IsAssignableFrom(fType))
            {
                Type elementType = fType.GetElementType();

                if (elementType == null)
                {
                    elementType = fType.GetProperty("Item").PropertyType;
                }
                if (elementType == typeof(WeakReference))
                    return true;

            }
            else if (fType == typeof(WeakReference))
            {
                return true;
            }
            
            return false;
        }
        public static SqoTypeInfo GetIndexSqoTypeInfo(Type t,FieldSqoInfo indexItemField)
        {

            List<FieldInfo> fi = new List<FieldInfo>();

            Dictionary<FieldInfo, PropertyInfo> automaticProperties = new Dictionary<FieldInfo, PropertyInfo>();

            FindFields(fi, automaticProperties, t);

            SqoTypeInfo ti = new SqoTypeInfo(t);

            int lengthOfRecord = sizeof(int); //=> +OID size;
            int i = 0;
            int nrOrder = 1;
            for (i = 0; i < fi.Count; i++)
            {
                int maxLength = -1;
                Type fType = fi[i].FieldType;
                bool isText = false;
                int fTypeId = -1;
                if (fType == typeof(object))//Item field
                {
                    fTypeId = indexItemField.AttributeTypeId;
                    fType = indexItemField.AttributeType;
                    if (indexItemField.AttributeType == typeof(string))
                    {
                        isText = true;
                        maxLength = GetSizeOfField(MetaExtractor.byteID);//get byte type size(simulate array byte[])
                    }
                }
                else
                {
                    fTypeId=GetAttributeType(fType);
                }
                if (isText)
                {
                    fTypeId = MetaExtractor.textID;
                }
                FieldSqoInfo ai = new FieldSqoInfo(fTypeId, fType);
                ai.Name = fi[i].Name;
                ai.FInfo = fi[i];
                ai.IsText = isText;
                ai.Header.Length = maxLength == -1 ? GetSizeOfField(fTypeId) : MetaHelper.PaddingSize(maxLength);
                if (fType.IsGenericType())
                {
                    Type genericTypeDef = fType.GetGenericTypeDefinition();
                    if (genericTypeDef == typeof(Nullable<>))
                    {
                        ai.Header.Length++;//increase Length to be able to store IsNull byte
                    }
                    else
                    {
                        ai.Header.Length += ExtraSizeForArray;//store OID from rawdata + number of elements
                        ai.AttributeTypeId += MetaExtractor.ArrayTypeIDExtra;
                    }
                }
                else if (fType.IsArray)
                {
                    ai.Header.Length += ExtraSizeForArray;//store OID from rawdata + number of elements
                    ai.AttributeTypeId += MetaExtractor.ArrayTypeIDExtra;
                }
                else if (ai.IsText)
                {
                    ai.Header.Length += ExtraSizeForArray;
                }
                ai.Header.RealLength = maxLength == -1 ? GetAbsoluteSizeOfField(fTypeId) : maxLength;
                ai.Header.PositionInRecord = lengthOfRecord;
                ai.Header.SizeOfName = ByteConverter.SerializeValueType(ai.Name, typeof(string), ti.Header.version).Length;
                ti.Fields.Add(ai);
                lengthOfRecord += ai.Header.Length;

                nrOrder++;


            }

            ti.Header.lengthOfRecord = lengthOfRecord;
            ti.Header.typeNameSize = ByteConverter.SerializeValueType(ti.TypeName, typeof(string), ti.Header.version).Length;
            ti.Header.NrFields = nrOrder - 1;


            ti.Header.headerSize = sizeof(int) + sizeof(int) + sizeof(long) + sizeof(int) + sizeof(int) + sizeof(int) + sizeof(int) + sizeof(int) + sizeof(int) + sizeof(int) + sizeof(int) + sizeof(int) +
                (ti.Header.NrFields * FieldSize) + ti.Header.typeNameSize;

            ti.Header.positionFirstRecord = ti.Header.headerSize;


            //cacheOfTypes[t] = ti;
            return ti;

        }
        public static void FindAddConstraints(SqoTypeInfo ti,FieldSqoInfo fi)
        {
            bool found = false;
            if (fi.FInfo != null)//if is null can be from OLD schema version and now field is missing
            {
                object[] customAttStr = fi.FInfo.GetCustomAttributes(typeof(UniqueConstraint), false);
                if (customAttStr.Length > 0)
                {
                    ti.UniqueFields.Add(fi);
                    found = true;
                }
                else
                {
                    PropertyInfo pi = MetaHelper.GetAutomaticProperty(fi.FInfo);
                    if (pi != null)
                    {
                        customAttStr = pi.GetCustomAttributes(typeof(UniqueConstraint), false);
                        if (customAttStr.Length > 0)
                        {
                            ti.UniqueFields.Add(fi);
                            found = true;
                        }
                    }
                }
                if (!found)//look in configuration
                {
                    if (Sqo.SiaqodbConfigurator.Constraints != null)
                    {
                        if (Sqo.SiaqodbConfigurator.Constraints.ContainsKey(ti.Type))
                        {
                            if (Sqo.SiaqodbConfigurator.Constraints[ti.Type].Contains(fi.Name))
                            {
                                ti.UniqueFields.Add(fi);
                            }
                        }
                    }
                }
            }
        }
        public static void FindAddIndexes(SqoTypeInfo ti, FieldSqoInfo fi)
        {
          
            bool found = false;
            if (fi.FInfo != null)
            {
                if (typeof(IList).IsAssignableFrom(fi.FInfo.FieldType))//ignore
                {
                    return;
                }
                object[] customAttStr = fi.FInfo.GetCustomAttributes(typeof(IndexAttribute), false);
                if (customAttStr.Length > 0)
                {
                    ti.IndexedFields.Add(fi);
                    found = true;
                }
                else
                {
                    PropertyInfo pi = MetaHelper.GetAutomaticProperty(fi.FInfo);
                    if (pi != null)
                    {
                        customAttStr = pi.GetCustomAttributes(typeof(IndexAttribute), false);
                        if (customAttStr.Length > 0)
                        {
                            ti.IndexedFields.Add(fi);
                            found = true;
                        }
                    }
                }
                if (!found)//look in configuration
                {
                    if (Sqo.SiaqodbConfigurator.Indexes != null)
                    {
                        if (Sqo.SiaqodbConfigurator.Indexes.ContainsKey(ti.Type))
                        {
                            if (Sqo.SiaqodbConfigurator.Indexes[ti.Type].Contains(fi.Name))
                            {
                                ti.IndexedFields.Add(fi);
                            }
                        }
                    }
                }
            }
        }
        public static bool IsTextType(int typeId)
        {
            return typeId == MetaExtractor.textID;
        }
        public static int GetSizeOfField(int typeId)
        {
            if (Sqo.SiaqodbConfigurator.EncryptedDatabase)
            {
                int blockSize = Sqo.SiaqodbConfigurator.Encryptor.GetBlockSize() / 8;
                if (typeId == intID || typeId == uintID || typeId == floatID )
                {
                    return blockSize; //normally 4
                }
                else if (typeId == shortID || typeId == ushortID || typeId == charID)
                {
                    return blockSize;//normally 2
                }
                else if (typeId == byteID || typeId == sbyteID || typeId == boolID)
                {
                    return blockSize; //normally 1
                }
                else if (typeId == longID || typeId == ulongID || typeId == doubleID || typeId == TimeSpanID || typeId == DateTimeID )
                {
                    return blockSize;
                }
                else if (typeId == decimalID || typeId == GuidID || typeId==DateTimeOffsetID)
                {
                    if (blockSize <= 16)
                        return 16;
                    else return blockSize;
                }
                else if (typeId == complexID || typeId==documentID)
                {
                    return 8;
                }

                else if (typeId == dictionaryID)
                {
                    return 17;
                }
                else if (typeId == jaggedArrayID || typeId==textID)
                {
                    return 13;
                }
                else if (typeId == stringID)
                {
                    return MetaHelper.PaddingSize(100);
                }
            }
            else
            {
                return GetAbsoluteSizeOfField(typeId);
               
            }
            return -1;
        }
        public static int GetAbsoluteSizeOfField(int typeId)
        {
            if (typeId == intID || typeId == uintID || typeId == floatID )
            {
                return 4;
            }
            else if (typeId == shortID || typeId == ushortID || typeId == charID)
            {
                return 2;
            }
            else if (typeId == byteID || typeId == sbyteID || typeId == boolID)
            {
                return 1;
            }
            else if (typeId == longID || typeId == ulongID || typeId == doubleID || typeId == TimeSpanID || typeId == DateTimeID || typeId == complexID || typeId==documentID)
            {
                return 8;
            }
            else if (typeId == decimalID || typeId == GuidID || typeId==DateTimeOffsetID)
            {
                return 16;
            }
            else if (typeId == dictionaryID)
            {
                return 17;//RawOID+NrElements+KeyTypeId+ValueTypeId + 1(null or not)
            }
            else if (typeId == jaggedArrayID || typeId == textID)
            {
                return 13;//itemTypeID+NrElements+arraySize+ 1(null or not)
            }
            else if (typeId == stringID)
            {
                return 100;
            }
            return -1;
        }
        public static int GetAttributeType(Type type)
        {

            if (type.IsGenericType() && !typeof(IDictionary).IsAssignableFrom(type))
            {
                // Nullable type?
                Type genericTypeDef = type.GetGenericTypeDefinition();
                if (genericTypeDef == typeof(Nullable<>) || typeof(IList).IsAssignableFrom(type))
                {
                    type = type.GetGenericArguments()[0];
                }
                
            }
            if (type.IsEnum())
            {
                Type enumType = Enum.GetUnderlyingType(type);
                return GetAttributeType(enumType);
            }
            if (type.IsArray)
            {
                type = type.GetElementType();
            }
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                return dictionaryID;
            }
            else if (typeof(IList).IsAssignableFrom(type))//jagged array
            {
                return jaggedArrayID;
            }
            else if (Cache.Cache.ContainsPrimitiveType(type))
            {
                return Cache.Cache.GetTypeID(type);
            }
            else if (type.IsClass() )
            {
                return MetaExtractor.complexID;
            }
            else
            {
                return -1;
            }
        }

        public static ObjectInfo GetObjectInfo(object o, SqoTypeInfo ti, Dotissi.Cache.MetaCache metaCache)
        {
            ObjectInfo oi = new ObjectInfo(ti, o);
            oi.Oid = metaCache.GetOIDOfObject(o, ti);
            oi.TickCount = MetaHelper.GetTickCountOfObject(o, ti.Type);

            foreach (FieldSqoInfo attKey in ti.Fields)
            {
#if SILVERLIGHT
                object objVal=null;
				try
				{
					objVal=MetaHelper.CallGetValue(attKey.FInfo,o,ti.Type);

				}
				catch (Exception ex)
				{
					throw new SiaqodbException("Override GetValue and SetValue methods of SqoDataObject-Silverlight limitation to private fields");
				}
#else
                object objVal = attKey.FInfo.GetValue(o);
#endif
                if (attKey.FInfo.FieldType == typeof(string))
                {
                    if (objVal == null)
                    {
                        objVal = string.Empty;
                    }

                }
                if (attKey.AttributeTypeId == MetaExtractor.documentID && objVal != null)
                {
                    Dotissi.MetaObjects.DocumentInfo dinfo = new Dotissi.MetaObjects.DocumentInfo();
                    dinfo.OID = metaCache.GetDocumentInfoOID(ti, o, attKey.Name);
                    dinfo.TypeName =MetaHelper.GetDiscoveringTypeName(attKey.AttributeType);
                    if (Sqo.SiaqodbConfigurator.DocumentSerializer == null)
                    {
                        throw new SiaqodbException("Document serializer is not set, use Sqo.SiaqodbConfigurator.SetDocumentSerializer method to set it");
                    }
                    dinfo.Document = Sqo.SiaqodbConfigurator.DocumentSerializer.Serialize(objVal);
                    objVal = dinfo;

                }
                oi.AtInfo[attKey] = objVal;



            }
            return oi;
        }
        public static Sqo.Utilities.ATuple<int,object> GetPartialObjectInfo(object o, SqoTypeInfo ti,string fieldName,Cache.MetaCache metaCache)
        {
            int oid = metaCache.GetOIDOfObject(o, ti);
            
            foreach (FieldSqoInfo attKey in ti.Fields)
            {
                if (attKey.Name != fieldName)
                    continue;
#if SILVERLIGHT
                object objVal=null;
				try
				{
					objVal=MetaHelper.CallGetValue(attKey.FInfo,o,ti.Type);

				}
				catch (Exception ex)
				{
					throw new SiaqodbException("Override GetValue and SetValue methods of SqoDataObject-Silverlight limitation to private fields");
				}
#else
                object objVal = attKey.FInfo.GetValue(o);
#endif
                if (attKey.FInfo.FieldType == typeof(string))
                {
                    if (objVal == null)
                    {
                        objVal = string.Empty;
                    }

                }
                if (attKey.AttributeTypeId == Dotissi.Meta.MetaExtractor.documentID && objVal != null)
                {
                    Dotissi.MetaObjects.DocumentInfo dinfo = new Dotissi.MetaObjects.DocumentInfo();
                    dinfo.OID = metaCache.GetDocumentInfoOID(ti, o, attKey.Name);
                    dinfo.TypeName = MetaHelper.GetDiscoveringTypeName(attKey.AttributeType);

                    if (Sqo.SiaqodbConfigurator.DocumentSerializer == null)
                    {
                        throw new SiaqodbException("Document serializer is not set, use Sqo.SiaqodbConfigurator.SetDocumentSerializer method to set it");
                    }
                    dinfo.Document = Sqo.SiaqodbConfigurator.DocumentSerializer.Serialize(objVal);
                    objVal = dinfo;

                }
                return new Sqo.Utilities.ATuple<int,object>(oid, objVal);

            }
            return null;
        }
		/// <summary>
		/// Return true if is same
		/// </summary>
		/// <param name="actual"></param>
		/// <param name="fromDB"></param>
		/// <returns></returns>
		public static bool CompareSqoTypeInfos(SqoTypeInfo actual, SqoTypeInfo fromDB)
		{
            if (fromDB.Header.version > -30)//version < 3.0 
            {
                return false;
            }
            
			if (actual.Header.NrFields != fromDB.Header.NrFields)
			{
				return false;
			}
			else
			{
				foreach (FieldSqoInfo fi in actual.Fields)
				{
					FieldSqoInfo fiDB = FindSqoField(fromDB.Fields, fi.Name);
					if (fiDB == null)
					{
						return false;
					}
					else
					{
						if (fi.AttributeTypeId != fiDB.AttributeTypeId)
						{
							return false;
						}
						else if (fi.Header.Length != fiDB.Header.Length)
						{
							return false;
						}
					}
				}
			}


			return true ;
		}
		private static FieldSqoInfo FindSqoField(List<FieldSqoInfo> list, string fieldName)
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
        public static void FindFields(ICollection<FieldInfo> fields,Dictionary<FieldInfo, PropertyInfo> automaticProperties, Type t)
        {
            var flags =BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

            foreach (var field in t.GetFields(flags))
            {
                // Ignore inherited fields + OID
                if (field.DeclaringType == t && field.Name != "oid" && field.Name != "<OID>k__BackingField")
                {

                    //get automatic properties
                    PropertyInfo pi = MetaHelper.GetAutomaticProperty(field);
                    if (pi != null)
                    {
                        bool found = false;
                        foreach (FieldInfo fiAuto in automaticProperties.Keys)
                        {
                            if (fiAuto.Name == field.Name)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (found)
                            continue;
                        automaticProperties[field] = pi;
                    }
                    fields.Add(field);

                }
            }

#if WinRT
            var baseType = t.GetBaseType();
#else
            var baseType = t.BaseType;
#endif
            if (baseType != null)
                FindFields(fields,automaticProperties, baseType);
        }
        public static FieldInfo FindField( Type t,string fieldName)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

            foreach (var field in t.GetFields(flags))
            {
                // Ignore inherited fields.
                if (field.DeclaringType == t)
                {
                    if (field.Name == fieldName)
                    {
                        return field;
                    }
                }
            }

#if WinRT
            var baseType = t.GetBaseType();
#else
            var baseType = t.BaseType;
#endif
            if (baseType != null)
                return FindField( baseType,fieldName);

            return null;
        }
		
		 
	}
}
