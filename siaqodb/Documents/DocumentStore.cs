using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sqo.Documents
{
    public class DocumentStore
    {
        Siaqodb siaqodb;
        Dictionary<string, Bucket> cache = new Dictionary<string, Bucket>();
        private readonly object _locker = new object();
        public DocumentStore(Siaqodb siaqodb)
        {
            this.siaqodb = siaqodb;
        }
        public IBucket this[string bucketName]
        {
            get
            {
                return this.GetBucket(bucketName);
            }
        }
        public IBucket GetBucket(string bucketName)
        {
            lock(_locker)
            {
                if (!cache.ContainsKey(bucketName))
                {
                    cache[bucketName] = new Bucket(bucketName, siaqodb);
                }
                return cache[bucketName];
            }
        }
        internal void ClearCache()
        {
            cache.Clear();
        }
    }
}
