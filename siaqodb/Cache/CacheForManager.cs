using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Meta;

namespace Sqo.Cache
{
    class CacheForManager
    {
        private Dictionary<string, SqoTypeInfo> cache = new Dictionary<string, SqoTypeInfo>();

        public void AddType(string type, SqoTypeInfo ti)
        {
            cache[type] = ti;
        }
        public SqoTypeInfo GetSqoTypeInfo(string t)
        {
            return cache[t];
        }
        public bool Contains(string type)
        {
            return cache.ContainsKey(type);
        }
    }
}
