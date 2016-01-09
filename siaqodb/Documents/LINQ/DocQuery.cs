using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Sqo.Documents
{
    public class DocQuery<T> : IDocQuery<T>
    {
        IBucket bucket;
        Query query;
        public DocQuery(IBucket bucket, Query query)
        {
            this.bucket = bucket;
            this.query = query;
        }
        public Query InnerQuery
        {
            get
            {
                return query;
            }
            internal set { query = value; }

        }
        public IBucket Bucket
        {
            get
            {
                return bucket;
            }
            internal set { bucket = value; }

        }

        public IEnumerator<T> GetEnumerator()
        {
            return bucket.Find(this.query).Cast<T>().GetEnumerator();
            
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return bucket.Find(this.query).GetEnumerator();
        }

        public List<V> ToObjects<V>()
        {
            List<V> list = new List<V>();
            var docs = bucket.Find(this.query);
            foreach (var doc in docs)
            {
                list.Add(doc.GetContent<V>());
            }
            return list;
        }
    }
}
