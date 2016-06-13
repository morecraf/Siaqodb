using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sqo.Documents;
using Sqo.Documents.Sync;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;

namespace SiaqodbCloud
{
    class AzureTableClient : ISiaqodbCloudClient
    {
        CloudTableClient tableClient;
        public AzureTableClient(CloudTableClient tableClient)
        {
            this.tableClient = tableClient;
        }
        public void Delete(string bucket, string key, string version)
        {
            CloudTable table = tableClient.GetTableReference(bucket);
            table.CreateIfNotExists();

            DynamicTableEntity den = new DynamicTableEntity(bucket, key);
            den.ETag = version;

            den.Properties.Add("soft_deleted", new EntityProperty(true));
            TableOperation insertOrRepl = TableOperation.Merge(den);
            try
            {
                var result = table.Execute(insertOrRepl);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 412)
                {
                    throw new ConflictException("There is a document with the same key and another version already stored");
                }
                else
                    throw;
            }

        }

        public Task DeleteAsync(string bucket, string key, string version)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            
        }

        public Document Get(string bucket, string key, string version = null)
        {
            CloudTable table = tableClient.GetTableReference(bucket);
            table.CreateIfNotExists();
            ChangeSet cset = new ChangeSet();
            string filterString = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, bucket),
                  TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, key));

            TableQuery query = new TableQuery().Where(filterString);

            IEnumerable<DynamicTableEntity> dens = table.ExecuteQuery(query);
            DynamicTableEntity en = dens.FirstOrDefault();
            if (en != null)
            {
                Document document = new Document();
                document.Key = en.RowKey;
                document.Version = en.ETag;
                if (en.Properties.ContainsKey("content"))
                {
                    document.Content = en.Properties["content"].BinaryValue;
                }
                foreach (string prKey in en.Properties.Keys)
                {
                    if (prKey == "soft_deleted" && en[prKey].BooleanValue == true)
                    {
                        return null;
                    }
                    if (prKey != "content" && prKey != "soft_deleted")
                    {
                        document.SetTag(prKey, en[prKey].PropertyAsObject);
                    }
                }
                return document;
            }
            return null;
        }

        public Task<Document> GetAsync(string bucket, string key, string version = null)
        {
            throw new NotImplementedException();
        }

        public ChangeSet GetChanges(string bucket, int limit, string anchor, string uploadAnchor)
        {
            return this.GetChanges(bucket, null, limit, anchor, uploadAnchor);
        }
    

        public ChangeSet GetChanges(string bucket, Filter filter, int limit, string anchor, string uploadAnchor)
        {
            CloudTable table = tableClient.GetTableReference(bucket);
            table.CreateIfNotExists();
            ChangeSet cset = new ChangeSet();
            cset.ChangedDocuments = new List<Document>();
            cset.DeletedDocuments = new List<DeletedDocument>();
            string filterString = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, bucket);
            if (!string.IsNullOrEmpty(anchor))
            {
                filterString = TableQuery.CombineFilters(filterString,
                TableOperators.And,
                TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThan, new DateTimeOffset(Convert.ToInt64(anchor), TimeSpan.Zero)));
            }
            if (filter != null)
            {
                string filterClient = this.GenerateFilter(filter);
                if (filterClient != null)
                {
                    filterString = TableQuery.CombineFilters(filterString, TableOperators.And, filterClient);
                }
            }
            
            TableQuery query = new TableQuery().Where(filterString).Take(limit);

            IEnumerable <DynamicTableEntity> dens = table.ExecuteQuery(query);
            DateTimeOffset maxTimeStamp = DateTimeOffset.MinValue;
            foreach (var den in dens)
            {
                if (den.Properties.ContainsKey("soft_deleted") && den.Properties["soft_deleted"].BooleanValue == true)
                {
                    //deleted  
                    cset.DeletedDocuments.Add(new DeletedDocument { Key = den.RowKey, Version = den.ETag });
                }
                else
                {
                        
                    Document document = new Document();
                    document.Key = den.RowKey;
                    document.Version = den.ETag;
                    document.Content = den.Properties["content"].BinaryValue;
                    foreach (string prKey in den.Properties.Keys)
                    {
                        if (prKey != "content" && prKey != "soft_deleted")
                        {
                            document.SetTag(prKey, den[prKey].PropertyAsObject);
                        }
                    }
                    cset.ChangedDocuments.Add(document);


                }
                if (maxTimeStamp < den.Timestamp)
                {
                    maxTimeStamp = den.Timestamp;
                }

            }
            if (maxTimeStamp!=DateTimeOffset.MinValue)
            {
                cset.Anchor = maxTimeStamp.UtcTicks.ToString();
            }
            return cset;
        }

        private string GenerateFilter(Filter query)
        {

            string tagName = query.TagName;
            if (query.TagName == "key")
            {
                tagName = "RowKey";
            }

            if (query.Value != null)
            {
                return GenerateFilterForOpVal(tagName, QueryComparisons.Equal, query.Value);
            }
            else if (query.Start != null && query.End==null)
            {
                return GenerateFilterForOpVal(tagName, QueryComparisons.GreaterThanOrEqual, query.Start);
            }
            else if (query.Start == null && query.End != null)
            {
                return GenerateFilterForOpVal(tagName, QueryComparisons.LessThanOrEqual, query.End);
            }
            else if (query.Start != null && query.End != null)
            {
                return TableQuery.CombineFilters(
                                     GenerateFilterForOpVal(tagName, QueryComparisons.GreaterThanOrEqual, query.Start),
                                         TableOperators.And,
                                     GenerateFilterForOpVal(tagName, QueryComparisons.LessThanOrEqual, query.End));
            }
            return null;

        }
        private string GenerateFilterForOpVal(string tagName,string oper,object value)
        {
            Type type = value.GetType();
            if (type == typeof(int) || type == typeof(long))
            {
                return TableQuery.GenerateFilterConditionForLong(tagName, oper, Convert.ToInt64(value));
            }
            else if (type == typeof(double) || type == typeof(float))
            {
                return TableQuery.GenerateFilterConditionForDouble(tagName, oper, Convert.ToDouble(value));

            }
            else if (type == typeof(DateTime))
            {
                return TableQuery.GenerateFilterConditionForDate(tagName, oper, (DateTime)value);
            }
            else if (type == typeof(bool))
            {
                return TableQuery.GenerateFilterConditionForBool(tagName, oper, (bool)value);
            }
            else if (type == typeof(string))
            {
                return TableQuery.GenerateFilterCondition(tagName, oper, (string)value);
            }
            return null;
        }
        public Task<ChangeSet> GetChangesAsync(string bucket, int limit, string anchor, string uploadAnchor)
        {
            throw new NotImplementedException();
        }

        public Task<ChangeSet> GetChangesAsync(string bucket, Filter query, int limit, string anchor, string uploadAnchor)
        {
            throw new NotImplementedException();
        }

        public BatchResponse Put(string bucket, ChangeSet batch)
        {
            CloudTable table = tableClient.GetTableReference(bucket);
            table.CreateIfNotExists();
            BatchResponse br = new BatchResponse();
            br.BatchItemResponses = new List<BatchItemResponse>();
            foreach (Document d in batch.ChangedDocuments)
            {
                DynamicTableEntity den = new DynamicTableEntity(bucket, d.Key);
                den.ETag = d.Version;
              
                den.Properties.Add("content", new EntityProperty(d.Content));
               
                foreach (string tagKey in d.Tags.Keys)
                {
                    Type type = d.Tags[tagKey].GetType();
                    if (type == typeof(long))
                    {
                        den.Properties.Add(tagKey,new EntityProperty( (long)d.Tags[tagKey]));
                    }
                    else if (type == typeof(double))
                    {
                        den.Properties.Add(tagKey, new EntityProperty((double)d.Tags[tagKey]));
                    }
                    else if (type == typeof(DateTime))
                    {

                        den.Properties.Add(tagKey, new EntityProperty((DateTime)d.Tags[tagKey]));
                    }
                    else if (type == typeof(string))
                    {
                        den.Properties.Add(tagKey, new EntityProperty((string)d.Tags[tagKey]));
                    }
                    else if (type == typeof(bool))
                    {
                        den.Properties.Add(tagKey, new EntityProperty((bool)d.Tags[tagKey]));
                    }
                }


                BatchItemResponse resp = new BatchItemResponse();
                resp.Key = den.RowKey;

                try
                {
                    TableOperation insertOrRepl = null;
                    if (den.ETag == null)
                    {
                        insertOrRepl = TableOperation.InsertOrReplace(den);
                    }
                    else
                    {
                        insertOrRepl = TableOperation.Replace(den);
                    }
                    var result = table.Execute(insertOrRepl);
                    resp.Version = den.ETag;

                }
                catch (StorageException ex)
                {
                    if (ex.RequestInformation.HttpStatusCode == 412)
                    {
                        resp.Version = d.Version;
                        resp.Error = "conflict";
                        resp.ErrorDesc = "conflict";
                        br.ItemsWithErrors++;
                    }
                    else
                        throw;
                }
                br.BatchItemResponses.Add(resp);


            }
            foreach (DeletedDocument d in batch.DeletedDocuments)
            {
                DynamicTableEntity den = new DynamicTableEntity(bucket, d.Key);
                den.ETag = d.Version;
                den.Properties = new Dictionary<string, EntityProperty>();
                den.Properties.Add("soft_deleted",new EntityProperty(true));
                BatchItemResponse resp = new BatchItemResponse();
                resp.Key = den.RowKey;
                try
                {
                   
                    TableOperation insertOrRepl = TableOperation.Replace(den);
                    var result = table.Execute(insertOrRepl);
                    resp.Version = den.ETag;

                }
                catch (StorageException ex)
                {
                    if (ex.RequestInformation.HttpStatusCode == 412)
                    {
                        resp.Error = "conflict";
                        resp.ErrorDesc = "conflict";
                    }
                    else
                    {
                        resp.Error = ex.Message;
                        resp.ErrorDesc =ex.Message;
                    }
                    resp.Version = d.Version;
                    br.ItemsWithErrors++;
                }
                br.BatchItemResponses.Add(resp);

            }
            br.Total = br.BatchItemResponses.Count;
            return br;
        }
          
        

        public StoreResponse Put(string bucket, Document d)
        {
            CloudTable table = tableClient.GetTableReference(bucket);
            table.CreateIfNotExists();

            DynamicTableEntity den = new DynamicTableEntity(bucket, d.Key);
            den.ETag = d.Version;

            den.Properties.Add("content", new EntityProperty(d.Content));

            foreach (string tagKey in d.Tags.Keys)
            {
                Type type = d.Tags[tagKey].GetType();
                if (type == typeof(long))
                {
                    den.Properties.Add(tagKey, new EntityProperty((long)d.Tags[tagKey]));
                }
                else if (type == typeof(double))
                {
                    den.Properties.Add(tagKey, new EntityProperty((double)d.Tags[tagKey]));
                }
                else if (type == typeof(DateTime))
                {

                    den.Properties.Add(tagKey, new EntityProperty((DateTime)d.Tags[tagKey]));
                }
                else if (type == typeof(string))
                {
                    den.Properties.Add(tagKey, new EntityProperty((string)d.Tags[tagKey]));
                }
                else if (type == typeof(bool))
                {
                    den.Properties.Add(tagKey, new EntityProperty((bool)d.Tags[tagKey]));
                }
            }


            StoreResponse resp = new StoreResponse();
            resp.Key = den.RowKey;

            try
            {
                TableOperation insertOrRepl = null;
                if (den.ETag == null)
                {
                    insertOrRepl = TableOperation.InsertOrReplace(den);
                }
                else
                {
                    insertOrRepl = TableOperation.Replace(den);
                }
                var result = table.Execute(insertOrRepl);
                resp.Version = den.ETag;
                return resp;

            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 412)
                {
                    throw new ConflictException("There is a document with the same key and another version already stored");
                }
                else
                    throw;
            }
        }

        public Task<BatchResponse> PutAsync(string bucket, ChangeSet batch)
        {
            throw new NotImplementedException();
        }

        public Task<StoreResponse> PutAsync(string bucket, Document obj)
        {
            throw new NotImplementedException();
        }
    }
   
}
