using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorClient.Bucket
{
    public class CryptonorBucket :IBucket
    {
        public Sqo.ISqoQuery<T> Cast<T>()
        {
            throw new NotImplementedException();
        }

        public Sqo.CryptonorObject Get(string key)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(string key)
        {
            throw new NotImplementedException();
        }

        public IList<Sqo.CryptonorObject> GetAll()
        {
            throw new NotImplementedException();
        }

        public IList<T> GetAll<T>()
        {
            throw new NotImplementedException();
        }

        public Sqo.ISqoQuery<Sqo.CryptonorObject> Query()
        {
            throw new NotImplementedException();
        }

        public void Store(Sqo.CryptonorObject obj)
        {
            throw new NotImplementedException();
        }

        public void Store(string key, object obj)
        {
            throw new NotImplementedException();
        }

        public void Store(string key, object obj, Dictionary<string, object> tags)
        {
            throw new NotImplementedException();
        }

        public void Store(string key, object obj, object tags = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(string key)
        {
            throw new NotImplementedException();
        }
    }
}
