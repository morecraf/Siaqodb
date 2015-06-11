using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dotissi.Meta;
using Dotissi.Utilities;
using Dotissi.Meta;

namespace Dotissi.Cache
{
    class CacheDocuments
    {
        Dictionary<SqoTypeInfo, ConditionalWeakTable> dict = new Dictionary<SqoTypeInfo, ConditionalWeakTable>();
        public void AddDocument(SqoTypeInfo ti,object parentObjOfDocument, string fieldName, int docinfoOid)
        {
            if (!dict.ContainsKey(ti))
            {
                dict.Add(ti, new ConditionalWeakTable());
            }
            dict[ti].Add(new DocumentCacheObject(parentObjOfDocument, fieldName),docinfoOid);
        }
        public int GetDocumentInfoOID(SqoTypeInfo ti, object parentObjOfDocument, string fieldName)
        {
            if (dict.ContainsKey(ti))
            {
                int oid;
                bool found = dict[ti].TryGetValue(new DocumentCacheObject(parentObjOfDocument, fieldName), out oid);
                if (found)
                    return oid;
            }
            return 0;
        }
    }
    class DocumentCacheObject
    {
        public object Parent { get; set; }
        public string FieldName { get; set; }
        public DocumentCacheObject(object parent,string fieldName)
        {
            this.Parent = parent;
            this.FieldName = fieldName;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Parent.GetHashCode();
                hash = hash * 31 + FieldName.GetHashCode();
                return hash;
            }
        }
        public override bool Equals(object obj)
        {
            DocumentCacheObject doc = obj as DocumentCacheObject;
            return this.Parent == doc.Parent && this.FieldName == doc.FieldName;
        }
    }
}
