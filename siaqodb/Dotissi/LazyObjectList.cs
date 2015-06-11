using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dotissi.Meta;
using Sqo;
using Sqo.Exceptions;
using Dotissi.Meta;


namespace Dotissi
{
#if KEVAST
    internal
#else
        public
#endif
    class LazyObjectList<T> : IObjectList<T>
    {
        List<int> oids;
        LazyEnumerator<T> enumerator;
        Siaqodb siaqodb;
        internal LazyObjectList(Siaqodb siaqodb, List<int> oids)
        {
            this.oids = oids;
            this.siaqodb = siaqodb;
        }
        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            if (this.enumerator == null)
            {
                this.enumerator = new LazyEnumerator<T>(this.siaqodb, oids);
            }
            return this.enumerator;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.enumerator;
        }

        #endregion

        #region IList<T> Members

        public int IndexOf(T item)
        {
            SqoTypeInfo ti = siaqodb.CheckDBAndGetSqoTypeInfo<T>();
            ObjectInfo objInfo = Dotissi.Meta.MetaExtractor.GetObjectInfo(item, ti,siaqodb.metaCache);
            return oids.IndexOf(objInfo.Oid);
        }

        public void Insert(int index, T item)
        {
            throw new SiaqodbException("LazyObjectList does not support this operation because objects are loaded on demand from db");
        }

        public void RemoveAt(int index)
        {
            throw new SiaqodbException("LazyObjectList does not support this operation because objects are loaded on demand from db");
        }

        public T this[int index]
        {
            get
            {
                T obj = siaqodb.LoadObjectByOID<T>(this.oids[index]);
                return obj;
            }
            set
            {
                throw new SiaqodbException("LazyObjectList does not support this operation because objects are loaded on demand from db");
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            throw new SiaqodbException("LazyObjectList does not support this operation because objects are loaded on demand from db");
        }

        public void Clear()
        {
            throw new SiaqodbException("LazyObjectList does not support this operation because objects are loaded on demand from db");
        }

        public bool Contains(T item)
        {
            SqoTypeInfo ti = siaqodb.CheckDBAndGetSqoTypeInfo<T>();
            ObjectInfo objInfo = Dotissi.Meta.MetaExtractor.GetObjectInfo(item, ti,siaqodb.metaCache);
            return oids.Contains(objInfo.Oid);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0; i < oids.Count; i++)
            {
                array[arrayIndex + i] = siaqodb.LoadObjectByOID<T>(oids[i]);  
            }
        }

        public int Count
        {
            get { return oids.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(T item)
        {
            throw new SiaqodbException("LazyObjectList does not support this operation because objects are loaded on demand from db");
        }

        #endregion
    }
    
}
