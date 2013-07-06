using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Meta;
using Sqo.Exceptions;

namespace Sqo.Utilities
{
#if !UNITY3D
    class ImportExport
    {
        
        public static void ExportToXML<T>(System.Xml.XmlWriter writer, IList<T> objects, Siaqodb siaqodb) 
        {
            SqoTypeInfo ti = siaqodb.GetSqoTypeInfo<T>();

            writer.WriteStartElement("SiaqodbObjects");

            writer.WriteStartElement("TypeDefinition");
            foreach (FieldSqoInfo fi in ti.Fields)
            {

                writer.WriteStartElement("member");
                writer.WriteAttributeString("type", fi.AttributeType.FullName);
                writer.WriteString(fi.Name);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteStartElement("objects");
            foreach (T obj in objects)
            {
                writer.WriteStartElement("object");
                ObjectInfo oi = MetaExtractor.GetObjectInfo(obj, ti,siaqodb.metaCache);
                foreach (FieldSqoInfo fi in ti.Fields)
                {
                    writer.WriteStartElement("memberValue");
                    Type typeElement = fi.AttributeType;
                    if (typeElement == typeof(char))
                    {
                        writer.WriteValue(oi.AtInfo[fi].ToString());
                    }
                    else if (typeElement == typeof(Guid))
                    {
                        writer.WriteValue(oi.AtInfo[fi].ToString());
                    }
                    else if (typeElement.IsEnum())
                    {
                        //writer.WriteValue(oi.AtInfo[fi]);
                        Type enumType = Enum.GetUnderlyingType(typeElement);

                        object realObject = Convertor.ChangeType(oi.AtInfo[fi], enumType);

                        writer.WriteValue(realObject);
                    }
                    else if (oi.AtInfo[fi] != null && oi.AtInfo[fi].GetType().IsEnum())
                    {
                        Type enumType = Enum.GetUnderlyingType(oi.AtInfo[fi].GetType());
                        object realObject = Convertor.ChangeType(oi.AtInfo[fi], enumType);
                        writer.WriteValue(realObject);
                    }
                    else
                    {
                        if (oi.AtInfo[fi] != null)
                        {
                            writer.WriteValue(oi.AtInfo[fi]);
                        }
                    }
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();

            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
        public static IObjectList<T> ImportFromXML<T>(System.Xml.XmlReader reader,Siaqodb siaqodb) 
        {
            ObjectTable obTable = new ObjectTable();
            reader.Read();
            reader.ReadStartElement("SiaqodbObjects");
            SqoTypeInfo ti = siaqodb.GetSqoTypeInfo<T>();
            bool colFinish = false;
            ObjectRow currentRow = null;
            int index = 0;
            Dictionary<int, Type> members = new Dictionary<int, Type>();
            while (reader.Read())
            {
                if (reader.IsStartElement() && reader.Name == "objects")
                {
                    colFinish = true;
                }

                if (reader.IsStartElement() && !colFinish)
                {
                    reader.MoveToFirstAttribute();
                    //string type = reader.Value;
                    Type t = Type.GetType(reader.Value);
                    reader.MoveToElement();

                    reader.ReadStartElement();
                    string columnName = reader.ReadContentAsString();
                    if (columnName == "OID")
                    {
                        throw new SiaqodbException("OID is set only internally, cannot be imported");
                    }
                    obTable.Columns.Add(columnName, index);
                    if (t.IsGenericType())
                    {
                        Type genericTypeDef = t.GetGenericTypeDefinition();
                        if (genericTypeDef == typeof(Nullable<>))
                        {
                            t = t.GetGenericArguments()[0];
                        }
                    }
                    members.Add(index, t);
                    index++;
                }
                if (reader.IsStartElement() && reader.Name == "object")
                {

                    currentRow = obTable.NewRow();
                    obTable.Rows.Add(currentRow);
                    index = 0;
                }
                if (reader.IsStartElement() && reader.Name == "memberValue")
                {
                    ReadMemberValue(currentRow, reader, index, members);
                    index++;
                    while (reader.Name == "memberValue")
                    {
                        ReadMemberValue(currentRow, reader, index, members);
                        index++;
                    }

                }
            }
            return ObjectTableHelper.CreateObjectsFromTable<T>(obTable, ti);
        }
        private static void ReadMemberValue(ObjectRow currentRow, System.Xml.XmlReader reader, int index, Dictionary<int, Type> members)
        {

            if (!reader.IsEmptyElement)
            {
                if (members[index] == typeof(char))
                {
                    string s = reader.ReadElementContentAsString();
                    if (!string.IsNullOrEmpty(s))
                    {
                        currentRow[index] = s[0];
                    }
                }
                else if (members[index] == typeof(Guid))
                {
                    string s = reader.ReadElementContentAsString();

                    currentRow[index] = new Guid(s);

                }
                else if (members[index].IsEnum())
                {
                    string s = reader.ReadElementContentAsString();

                    Type enumType = Enum.GetUnderlyingType(members[index]);
                   
                    object realObject = Convertor.ChangeType(s, enumType);
                   
                    currentRow[index] = Enum.ToObject(members[index], realObject);

                }
                else
                {
                    currentRow[index] = reader.ReadElementContentAs(members[index], null);
                }
            }
            else
            {

                reader.ReadElementContentAsString();

            }
            //reader.MoveToElement();

        }
    }
#endif
}
