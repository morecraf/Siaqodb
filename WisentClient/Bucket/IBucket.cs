using Sqo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorClient
{
    public interface IBucket
    {
        ISqoQuery<T> Cast<T>();
        CryptonorObject Get(string key);
        T Get<T>(string key);
        System.Collections.Generic.IList<CryptonorObject> GetAll();
        System.Collections.Generic.IList<T> GetAll<T>();
        ISqoQuery<CryptonorObject> Query();
        void Store(CryptonorObject obj);
        void Store(string key, object obj);
        void Store(string key, object obj, System.Collections.Generic.Dictionary<string, object> tags);
        void Store(string key, object obj, object tags = null);
        void Delete(string key);
    }
}
