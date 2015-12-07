﻿using Sqo.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sqo.Documents
{
    public interface IBucket
    {
        Document Load(string key);
        T Load<T>(string key);
        IList<Document> Find(Query query);
        IList<Document> LoadAll();
        IList<Document> LoadAll(int skip, int limit);
        void Store(Document doc);
        void Store(Document doc, ITransaction transaction);
        void Store(string key, object obj);
        void Store(string key, object obj, Dictionary<string, object> tags);
        void Store(string key, object obj, object tags = null);
        void Store(object obj, object tags = null);
        void StoreBatch(IList<Document> docs);
        void Delete(string key);
        void Delete(Document doc);
        string BucketName { get; set; }
        IDocQuery<T> Cast<T>() where T : Document;
        IDocQuery<T> Query<T>() where T : Document;
    }
}
