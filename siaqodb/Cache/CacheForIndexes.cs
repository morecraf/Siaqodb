using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Meta;
using Sqo.Indexes;
using Sqo.Utilities;

namespace Sqo.Cache
{
    class CacheForIndexes
    {
        Dictionary<SqoTypeInfo, Dictionary<FieldSqoInfo, IBTree>> cache = new Dictionary<SqoTypeInfo, Dictionary<FieldSqoInfo, IBTree>>();

        public void Add(SqoTypeInfo ti, Dictionary<FieldSqoInfo, IBTree> dictionary)
        {
            cache[ti] = dictionary;
        }
        public IBTree GetIndex(SqoTypeInfo type,FieldSqoInfo fi)
        {
            if (cache.ContainsKey(type))
            {
                if (cache[type].ContainsKey(fi))
                {
                    return cache[type][fi];
                }
            }
            return null;
        }
        public IBTree GetIndex(SqoTypeInfo type, string fieldName)
        {
            if (cache.ContainsKey(type))
            {
                FieldSqoInfo fi = MetaHelper.FindField(type.Fields, fieldName);
                if (fi != null)
                    return this.GetIndex(type, fi);
            }
            return null;
        }
        public bool ContainsType(SqoTypeInfo type)
        {
            return cache.ContainsKey(type);
            
        }
        public bool RemoveType(SqoTypeInfo type)
        {
            if (cache.ContainsKey(type))
            {
                return cache.Remove(type);
            }
            return false;
        }
        public Dictionary<FieldSqoInfo, IBTree> GetIndexes(SqoTypeInfo ti)
        {
            if (cache.ContainsKey(ti))
            {
                return cache[ti];
            }
            return null;
        }
    }
}
