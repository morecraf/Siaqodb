using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sqo;
using Sqo.Core;
using Sqo.Meta;
using Sqo.Exceptions;
using Sqo.Utilities;
using System.Collections;
using System.Reflection;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Sqo.Core
{

    [Obfuscation(Feature = "Apply to member * when event: renaming", Exclude = true)]
    partial class  ObjectSerializer
    {
        string filePath;
        ISqoFile file;
        readonly object _syncRoot = new object();
       
        public ObjectSerializer(string filePath,bool useElevatedTrust)
        {
            this.filePath = filePath;
            file = FileFactory.Create(filePath, false,useElevatedTrust);
        }
        private byte[] SerializeTypeToBuffer(SqoTypeInfo ti)
        {
            byte[] headerSize = ByteConverter.IntToByteArray(ti.Header.headerSize);
            byte[] typeNameSize = ByteConverter.IntToByteArray(ti.Header.typeNameSize);
            byte[] typeName = ByteConverter.SerializeValueType(ti.TypeName, typeof(string), ti.Header.version);
            byte[] lastUpdate = ByteConverter.SerializeValueType(DateTime.Now, typeof(DateTime), ti.Header.version);
            byte[] numberOfRecords = ByteConverter.IntToByteArray(ti.Header.numberOfRecords);
            byte[] positionFirstRecord = ByteConverter.IntToByteArray(ti.Header.positionFirstRecord);
            byte[] lengthOfRecord = ByteConverter.IntToByteArray(ti.Header.lengthOfRecord);
            byte[] version = ByteConverter.IntToByteArray(ti.Header.version);
            byte[] nrFields = ByteConverter.IntToByteArray(ti.Header.NrFields);
            byte[] TID = ByteConverter.IntToByteArray(ti.Header.TID);
            byte[] unused1 = ByteConverter.IntToByteArray(ti.Header.Unused1);
            byte[] unused2 = ByteConverter.IntToByteArray(ti.Header.Unused2);
            byte[] unused3 = ByteConverter.IntToByteArray(ti.Header.Unused3);

            byte[] tArray = Combine(headerSize, typeNameSize, typeName, lastUpdate, numberOfRecords,
                                        positionFirstRecord, lengthOfRecord, version,
                                        nrFields, TID, unused1, unused2, unused3);
            int fieldIndex = 1;
            byte[][] fullArray = new byte[ti.Fields.Count + 1][];
            fullArray[0] = tArray;
            foreach (FieldSqoInfo ai in ti.Fields)
            {
                byte[] SizeOfName = ByteConverter.IntToByteArray(ai.Header.SizeOfName);
                byte[] aiName = ByteConverter.SerializeValueType(ai.Name, typeof(string), ti.Header.version);
                Array.Resize<byte>(ref aiName, 200);
                byte[] attLength = ByteConverter.IntToByteArray(ai.Header.Length);
                byte[] positionInRecord = ByteConverter.IntToByteArray(ai.Header.PositionInRecord);
                byte[] nrOrder = ByteConverter.IntToByteArray(ai.Header.RealLength);
                byte[] typeId = ByteConverter.IntToByteArray(ai.AttributeTypeId);

                fullArray[fieldIndex] = Combine(SizeOfName, aiName, attLength, positionInRecord, nrOrder, typeId);

                fieldIndex++;

            }
            return Combine(fullArray);
        }
        public void SerializeType(SqoTypeInfo ti)
        {

            file.Write(0,SerializeTypeToBuffer(ti));

        }
#if ASYNC
        public async Task SerializeTypeAsync(SqoTypeInfo ti)
        {
            await file.WriteAsync(0, SerializeTypeToBuffer(ti)).ConfigureAwait(false);

        }
#endif
        private SqoTypeInfo DeserializeSqoTypeInfoFromBuffer(byte[] readFullSqoTypeInfo, bool loadRealType)
        {
            SqoTypeInfo tInfo = new SqoTypeInfo();
            tInfo.Header.headerSize = readFullSqoTypeInfo.Length;
            try
            {
                //reader.Close();
                byte[] typeNameSize = GetBuffer(readFullSqoTypeInfo, 4, 4);
                tInfo.Header.typeNameSize = ByteConverter.ByteArrayToInt(typeNameSize);
                //read versionFirst
                byte[] version = GetBuffer(readFullSqoTypeInfo, tInfo.Header.typeNameSize + 28, 4);
                tInfo.Header.version = ByteConverter.ByteArrayToInt(version);

                byte[] typeName = GetBuffer(readFullSqoTypeInfo, 8, tInfo.Header.typeNameSize);
                tInfo.TypeName = (string)ByteConverter.DeserializeValueType(typeof(string), typeName, tInfo.Header.version);

                if (loadRealType)
                {
#if SILVERLIGHT
                        string[] tinfoArr=tInfo.TypeName.Split(',');
                        string fullTypeName = tInfo.TypeName;
                        if (tinfoArr.Length == 2 && !tInfo.TypeName.StartsWith("Sqo.Indexes.BTreeNode") && !tInfo.TypeName.StartsWith("KeVaSt.BTreeNode"))//written with .net version
                        {
                            fullTypeName = tInfo.TypeName + ", Version=0.0.0.1,Culture=neutral, PublicKeyToken=null";
                            tInfo.Type = Type.GetType(fullTypeName);
                            tInfo.TypeName = tInfo.Type.AssemblyQualifiedName;
                        }
                        else
                        {
                            tInfo.Type = Type.GetType(fullTypeName);
                        }
                       
#else
                    string[] tinfoArr = null;
                    int indexOfGenericsEnd = tInfo.TypeName.LastIndexOf(']');
                    if (indexOfGenericsEnd > 0)
                    {
                        string substringStart = tInfo.TypeName.Substring(0, indexOfGenericsEnd);
                        string substringEnd = tInfo.TypeName.Substring(indexOfGenericsEnd, tInfo.TypeName.Length - indexOfGenericsEnd);
                        tinfoArr = substringEnd.Split(',');
                        tinfoArr[0] = substringStart + "]";
                    }
                    else
                    {
                        tinfoArr = tInfo.TypeName.Split(',');
                    }
                    string fullTypeName = tInfo.TypeName;
                    if (tinfoArr.Length > 2 && !tInfo.TypeName.StartsWith("Sqo.Indexes.BTreeNode") && !tInfo.TypeName.StartsWith("KeVaSt.BTreeNode"))//written with Silevrlight version
                    {
                        fullTypeName = tinfoArr[0] + "," + tinfoArr[1];
                        tInfo.Type = Type.GetType(fullTypeName);
                        tInfo.TypeName = fullTypeName;
                    }
                    else
                    {
                        tInfo.Type = Type.GetType(tInfo.TypeName);
                    }
#endif
                }
                byte[] lastUpdate = GetBuffer(readFullSqoTypeInfo, tInfo.Header.typeNameSize + 8, 8);
                tInfo.Header.lastUpdated = (DateTime)ByteConverter.DeserializeValueType(typeof(DateTime), lastUpdate, tInfo.Header.version);

                byte[] nrRecords = GetBuffer(readFullSqoTypeInfo, tInfo.Header.typeNameSize + 16, 4);
                tInfo.Header.numberOfRecords = ByteConverter.ByteArrayToInt(nrRecords);

                byte[] positionFirstRecord = GetBuffer(readFullSqoTypeInfo, tInfo.Header.typeNameSize + 20, 4);
                tInfo.Header.positionFirstRecord = ByteConverter.ByteArrayToInt(positionFirstRecord);

                byte[] lengthRecord = GetBuffer(readFullSqoTypeInfo, tInfo.Header.typeNameSize + 24, 4);
                tInfo.Header.lengthOfRecord = ByteConverter.ByteArrayToInt(lengthRecord);


                int currentPosition = tInfo.Header.typeNameSize + 32;
                byte[] nrFields = GetBuffer(readFullSqoTypeInfo, currentPosition, 4);
                tInfo.Header.NrFields = ByteConverter.ByteArrayToInt(nrFields);

                if (tInfo.Header.version <= -30) //version >= 3.0
                {
                    currentPosition += 4;
                    byte[] TID = GetBuffer(readFullSqoTypeInfo, currentPosition, 4);
                    tInfo.Header.TID = ByteConverter.ByteArrayToInt(TID);

                    currentPosition += 4;
                    byte[] unused1 = GetBuffer(readFullSqoTypeInfo, currentPosition, 4);
                    tInfo.Header.Unused1 = ByteConverter.ByteArrayToInt(unused1);

                    currentPosition += 4;
                    byte[] unused2 = GetBuffer(readFullSqoTypeInfo, currentPosition, 4);
                    tInfo.Header.Unused2 = ByteConverter.ByteArrayToInt(unused2);

                    currentPosition += 4;
                    byte[] unused3 = GetBuffer(readFullSqoTypeInfo, currentPosition, 4);
                    tInfo.Header.Unused3 = ByteConverter.ByteArrayToInt(unused3);
                }


                for (int i = 0; i < tInfo.Header.NrFields; i++)
                {

                    FieldSqoInfo ai = new FieldSqoInfo();
                    int currentPositionField = (i * MetaExtractor.FieldSize) + currentPosition + 4;
                    byte[] sizeOfName = GetBuffer(readFullSqoTypeInfo, currentPositionField, 4);
                    ai.Header.SizeOfName = ByteConverter.ByteArrayToInt(sizeOfName);

                    currentPositionField += 4;
                    byte[] name = GetBuffer(readFullSqoTypeInfo, currentPositionField, ai.Header.SizeOfName);
                    ai.Name = (string)ByteConverter.DeserializeValueType(typeof(string), name, tInfo.Header.version);

                    currentPositionField += 200;
                    byte[] length = GetBuffer(readFullSqoTypeInfo, currentPositionField, 4);
                    ai.Header.Length = ByteConverter.ByteArrayToInt(length);

                    currentPositionField += 4;
                    byte[] position = GetBuffer(readFullSqoTypeInfo, currentPositionField, 4);
                    ai.Header.PositionInRecord = ByteConverter.ByteArrayToInt(position);

                    currentPositionField += 4;
                    byte[] nrOrder = GetBuffer(readFullSqoTypeInfo, currentPositionField, 4);
                    ai.Header.RealLength = ByteConverter.ByteArrayToInt(nrOrder);

                    currentPositionField += 4;
                    byte[] typeId = GetBuffer(readFullSqoTypeInfo, currentPositionField, 4);
                    ai.AttributeTypeId = ByteConverter.ByteArrayToInt(typeId);

                    if (loadRealType)
                    {

                        ai.FInfo = MetaExtractor.FindField(tInfo.Type, ai.Name);
                        MetaExtractor.FindAddConstraints(tInfo, ai);
                        MetaExtractor.FindAddIndexes(tInfo, ai);

                    }
                    if (ai.AttributeTypeId == MetaExtractor.complexID || ai.AttributeTypeId == MetaExtractor.dictionaryID)
                    {
                        if (loadRealType)
                        {
                            ai.AttributeType = ai.FInfo.FieldType;
                        }
                    }
                    else if (ai.Header.Length - 1 == MetaExtractor.GetSizeOfField(ai.AttributeTypeId))//is Nullable<>
                    {
                        Type fGen = typeof(Nullable<>);
                        ai.AttributeType = fGen.MakeGenericType(Cache.Cache.GetTypebyID(ai.AttributeTypeId));
                    }
                    else if (MetaExtractor.IsTextType(ai.AttributeTypeId))
                    {
                        ai.AttributeType = typeof(string);
                        ai.IsText = true;
                    }
                    else if (ai.AttributeTypeId > MetaExtractor.ArrayTypeIDExtra)//is IList<> or Array
                    {
                        if (loadRealType)
                        {
                            ai.AttributeType = ai.FInfo.FieldType;
                        }
                        else
                        {
                            if (ai.AttributeTypeId - MetaExtractor.ArrayTypeIDExtra == MetaExtractor.complexID
                                || ai.AttributeTypeId - MetaExtractor.ArrayTypeIDExtra == MetaExtractor.jaggedArrayID)
                            {

                            }
                            else
                            {
                                Type elementType = Cache.Cache.GetTypebyID(ai.AttributeTypeId);
#if CF
                                ai.AttributeType = Array.CreateInstance(elementType,0).GetType();
#else
                                ai.AttributeType = elementType.MakeArrayType();
#endif
                            }

                        }
                        if (ai.AttributeTypeId - MetaExtractor.ArrayTypeIDExtra == MetaExtractor.textID)
                        {
                            ai.IsText = true;
                        }
                           
                    }
                    else if (ai.AttributeTypeId > MetaExtractor.FixedArrayTypeId)//is IList<> or Array
                    {
                        if (loadRealType)
                        {
                            ai.AttributeType = ai.FInfo.FieldType;
                        }
                        else
                        {
                            if (ai.AttributeTypeId - MetaExtractor.FixedArrayTypeId == MetaExtractor.complexID
                                || ai.AttributeTypeId - MetaExtractor.FixedArrayTypeId == MetaExtractor.jaggedArrayID)
                            {

                            }
                            else
                            {
                                Type elementType = Cache.Cache.GetTypebyID(ai.AttributeTypeId);
#if CF
                                ai.AttributeType = Array.CreateInstance(elementType,0).GetType();
#else
                                ai.AttributeType = elementType.MakeArrayType();
#endif
                            }

                        }
                    }
                    else
                    {
                        ai.AttributeType = Cache.Cache.GetTypebyID(ai.AttributeTypeId);
                    }
                    tInfo.Fields.Add(ai);

                }


                return tInfo;
            }


            catch (Exception ex)
            {
                SiaqodbConfigurator.LogMessage("File:" + this.filePath + " is not a valid Siaqodb database file,skipped; error:"+ex.ToString(), VerboseLevel.Info);
            }
            return null;
        }
        public SqoTypeInfo DeserializeSqoTypeInfo(bool loadRealType)
        {

            byte[] headerSizeB = new byte[4];
            file.Read(0, headerSizeB);
            int headerSize = ByteConverter.ByteArrayToInt(headerSizeB);
            byte[] readFullSqoTypeInfo = new byte[headerSize];
            file.Read(0, readFullSqoTypeInfo);
            return DeserializeSqoTypeInfoFromBuffer(readFullSqoTypeInfo, loadRealType);


        }
        #if ASYNC

        public async Task<SqoTypeInfo> DeserializeSqoTypeInfoAsync(bool loadRealType)
        {
            byte[] headerSizeB = new byte[4];
            await file.ReadAsync(0, headerSizeB).ConfigureAwait(false);
            int headerSize = ByteConverter.ByteArrayToInt(headerSizeB);
            byte[] readFullSqoTypeInfo = new byte[headerSize];
            await file.ReadAsync(0, readFullSqoTypeInfo).ConfigureAwait(false);
            return DeserializeSqoTypeInfoFromBuffer(readFullSqoTypeInfo, loadRealType);
        }
#endif
        public void Open(bool useElevatedTrust)
        {

            file = FileFactory.Create(filePath, false, useElevatedTrust);
        }
        public void MakeEmpty()
        {
            file.Length = 0;
        }
        public void SetLength(long newLength)
        {
            file.Length = newLength;
        }

        public void Close()
        {
            file.Flush();
            file.Close();
        }
        #if ASYNC
        public async Task CloseAsync()
        {
            await file.FlushAsync().ConfigureAwait(false);
            file.Close();
        }
        #endif
        
        public bool IsClosed
        {
            get { return file.IsClosed; }
        }
        public void Flush()
        {
            lock (file)
            {
                file.Flush();
            }
        }
#if ASYNC
        public async Task FlushAsync()
        {

            await file.FlushAsync().ConfigureAwait(false);

        }
#endif
        private byte[] GetBuffer(byte[] readFullSqoTypeInfo, int position, int size)
        {
            byte[] b = new byte[size];
            Array.Copy(readFullSqoTypeInfo, position, b, 0, size);
            return b;
        }
        public static byte[] Combine(params byte[][] arrays)
        {
            int totalLegth = 0;
            foreach (byte[] data in arrays)
            {
                totalLegth += data.Length;
            }
            byte[] ret = new byte[totalLegth];
            int offset = 0;
            foreach (byte[] data in arrays)
            {
                Array.Copy(data, 0, ret, offset, data.Length);
                offset += data.Length;
            }
            return ret;
        }
       
        private FieldSqoInfo FindField(List<FieldSqoInfo> list, string fieldName)
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

        
    }
    internal class ComplexObjectEventArgs : EventArgs
    {
        public object ComplexObject { get; set; }
        public Type ParentType { get; set; }
        public string FieldName { get; set; }
        public ComplexObjectEventArgs(object obj,bool returnOnlyOid)
        {
            this.ComplexObject = obj;
            this.ReturnOnlyOid_TID = returnOnlyOid;
        }
        public ComplexObjectEventArgs(int OID,int TID)
        {
            this.TID = TID;
            this.SavedOID = OID;
        }
        public ComplexObjectEventArgs(bool justSetOID, ObjectInfo objInfo)
        {
            this.JustSetOID = justSetOID;
            this.ObjInfo = objInfo;
        }
        public ObjectInfo ObjInfo { get; set; }
        public int SavedOID { get; set; }
        public int TID { get; set; }
        public bool ReturnOnlyOid_TID { get; set; }
        public bool JustSetOID { get; set; }
    }
#if ASYNC
    internal delegate Task ComplexObjectEventHandler(object sender, ComplexObjectEventArgs args);
#endif
}
