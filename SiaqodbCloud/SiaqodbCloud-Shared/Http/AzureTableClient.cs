#if !UNITY3D
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Documents;
using Sqo.Documents.Sync;

using System.Threading.Tasks;
using System.Threading;
using Dotissi.AzureTable.LiteClient;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Dotissi.AzureTable.LiteClient.Exceptions;
using System.Dynamic;

namespace SiaqodbCloud
{
    class AzureTableHTTPClient : ISiaqodbCloudClient
    {
        AzureTableClient tableClient;
        public AzureTableHTTPClient(AzureTableClient tableClient)
        {
            this.tableClient = tableClient;
        }

        private Dictionary<string, bool> createdTables = new Dictionary<string, bool>();
        private async Task<AzureTable> GetTableReference(string bucket)
        {
            var table = tableClient.GetTableReference(bucket);
            if (!createdTables.ContainsKey(bucket))
            {
                bool created = await table.ExistsTableAsync();
                if (!created)
                {
                    await table.CreateTableAsync();
                }
                createdTables[bucket] = true;
            }
            return table;
        }
        public async Task<Document> GetAsync(string bucket, string key, string version = null)
        {
            AzureTable table = await this.GetTableReference(bucket);
            ChangeSet cset = new ChangeSet();
            JObject en = await table.FindOneAsync<JObject>(bucket, key);
            
            if (en != null)
            {
                Document document = new Document();
                document.Key = en["RowKey"].Value<string>();
                document.Version = en["odata.etag"].Value<string>();
                var contentProp = en.Property("content");
                if (contentProp != null)
                {
                    document.Content = en["content"].Value<byte[]>();
                }
                foreach (JProperty property in en.Properties())
                {
                    if (property.Name == "soft_deleted" && en["soft_deleted"].Value<bool>() == true)
                    {
                        return null;
                    }
                    if (property.Name != "content" && property.Name != "soft_deleted" && property.Name != "Timestamp" && property.Name != "RowKey" && property.Name != "PartitionKey" && !property.Name.StartsWith("odata."))
                    {
                        document.SetTag(property.Name, ((JValue)en[property.Name]).Value);
                    }
                }
                return document;
            }
            return null;
        }

        public Task<ChangeSet> GetChangesAsync(string bucket, int limit, string anchor, string uploadAnchor)
        {
            return GetChangesAsync(bucket, null, limit, anchor, uploadAnchor);
        }

        public async Task<ChangeSet> GetChangesAsync(string bucket, Filter filter, int limit, string anchor, string uploadAnchor)
        {
            AzureTable table = await this.GetTableReference(bucket);

            string filterStr = this.GenerateQuery(bucket, filter, limit, anchor);
            IEnumerable<JObject> dens = await table.FindAllAsync<JObject>(filterStr);
            return PrepareChangeSet(dens);
        }


        private ChangeSet PrepareChangeSet(IEnumerable<JObject> dens)
        {
            ChangeSet cset = new ChangeSet();
            cset.ChangedDocuments = new List<Document>();
            cset.DeletedDocuments = new List<DeletedDocument>();
            DateTimeOffset maxTimeStamp = DateTimeOffset.MinValue;
            foreach (var den in dens)
            {
                JProperty existsSoftDeleted = den.Properties().Where(a => a.Name == "soft_deleted").FirstOrDefault();
                DateTimeOffset timestamp = den["Timestamp"].Value<DateTime>();
                if (existsSoftDeleted != null && den["soft_deleted"].Value<bool>() == true)
                {
                    string k = den["RowKey"].Value<string>();
                    string etag = den["odata.etag"].Value<string>();

                    cset.DeletedDocuments.Add(new DeletedDocument { Key = k, Version = etag });
                }
                else
                {

                    Document document = new Document();
                    document.Key = den["RowKey"].Value<string>();
                    document.Version = den["odata.etag"].Value<string>();
                    document.Content = den["content"].Value<byte[]>();
                    foreach (JProperty property in den.Properties())
                    {
                        if (property.Name != "content" && property.Name != "soft_deleted" && property.Name != "Timestamp" && property.Name != "RowKey" && property.Name != "PartitionKey" && !property.Name.StartsWith("odata."))
                        {
                            document.SetTag(property.Name, ((JValue)den[property.Name]).Value);
                        }
                    }
                    cset.ChangedDocuments.Add(document);
                }
                if (maxTimeStamp < timestamp)
                {
                    maxTimeStamp = timestamp;
                }

            }
            if (maxTimeStamp != DateTimeOffset.MinValue)
            {
                cset.Anchor = maxTimeStamp.UtcTicks.ToString();
            }
            return cset;
        }
        private string GenerateQuery(string bucket, Filter filter, int limit, string anchor)
        {
            StringBuilder sb = new StringBuilder();
            string filterString = string.Format("PartitionKey eq '{0}'", bucket);
            sb.Append(filterString);
            if (!string.IsNullOrEmpty(anchor))
            {
                sb.Append(string.Format(" and Timestamp gt datetime'{0}'", (new DateTimeOffset(Convert.ToInt64(anchor), TimeSpan.Zero)).UtcDateTime.ToString("o", CultureInfo.InvariantCulture)));
            }
            if (filter != null)
            {
                string filterClient = this.GenerateFilter(filter);
                if (filterClient != null)
                {
                    sb.Append(filterClient);
                }
            }

            return sb.ToString();
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
                return GenerateFilterForOpVal(tagName, "eq", query.Value);
            }
            else if (query.Start != null && query.End == null)
            {
                return GenerateFilterForOpVal(tagName, "ge", query.Start);
            }
            else if (query.Start == null && query.End != null)
            {
                return GenerateFilterForOpVal(tagName, "le", query.End);
            }
            else if (query.Start != null && query.End != null)
            {
                return GenerateFilterForOpVal(tagName, "ge", query.Start) + GenerateFilterForOpVal(tagName, "le", query.End);

            }
            return null;

        }
        private string GenerateFilterForOpVal(string tagName, string oper, object value)
        {
            Type type = value.GetType();
            if (type == typeof(int) || type == typeof(long))
            {
                return string.Format(" and {0} {1} {2}L", tagName, oper, value);
            }
            else if (type == typeof(double) || type == typeof(float))
            {
                return string.Format(" and {0} {1} {2}", tagName, oper, value);

            }
            else if (type == typeof(DateTime))
            {
                DateTime date = (DateTime)value;

                return string.Format(" and {0} {1} datetime'{2}'", tagName, oper, date.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture));
            }
            else if (type == typeof(bool))
            {
                return string.Format(" and {0} {1} {2}", tagName, oper, value.ToString().ToLower());
            }
            else if (type == typeof(string))
            {
                return string.Format(" and {0} {1} '{2}'", tagName, oper, value);
            }
            return null;
        }


        public async Task<BatchResponse> PutAsync(string bucket, ChangeSet batch)
        {
            AzureTable table = await this.GetTableReference(bucket);

            BatchResponse br = new BatchResponse();
            br.BatchItemResponses = new List<BatchItemResponse>();
            foreach (Document d in batch.ChangedDocuments)
            {
                BatchItemResponse resp = new BatchItemResponse { Key = d.Key };
                try
                {
                    TableResult result = null;
                    IDictionary<String, Object> myEn = new ExpandoObject();
                    myEn["RowKey"] = d.Key;
                    myEn["PartitionKey"] = bucket;
                    myEn["content"] = d.Content;

                    foreach (var tagKey in d.Tags)
                    {
                        
                        myEn.Add(tagKey);
                    }
                    if (d.Version == null)
                    {
                        result = await table.InsertOrReplaceAsync(myEn);
                    }
                    else
                    {
                        result = await table.ReplaceAsync(myEn, d.Version);
                    }
                    resp.Version = result.ETag;

                }
                catch (StorageException ex)
                {
                    SetError(resp, ex);
                    resp.Version = d.Version;
                    br.ItemsWithErrors++;

                }
                br.BatchItemResponses.Add(resp);


            }
            foreach (DeletedDocument d in batch.DeletedDocuments)
            {
                IDictionary<String, Object> myEn = new ExpandoObject();
                myEn["RowKey"] = d.Key;
                myEn["PartitionKey"] = bucket;
                myEn["soft_deleted"] = true;
                BatchItemResponse resp = new BatchItemResponse { Key = d.Key };
                try
                {
                    var result = await table.ReplaceAsync(myEn, d.Version);
                    resp.Version = result.ETag;

                }
                catch (StorageException ex)
                {
                    SetError(resp, ex);
                    resp.Version = d.Version;
                    br.ItemsWithErrors++;
                }
                br.BatchItemResponses.Add(resp);

            }
            br.Total = br.BatchItemResponses.Count;
            return br;
        }

        private void SetError(BatchItemResponse resp, StorageException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
            {
                resp.Error = "conflict";
                resp.ErrorDesc = "conflict";
            }
            else
            {
                resp.Error = ex.Message;
                resp.ErrorDesc = ex.Message;
            }
        }


        public async Task<StoreResponse> PutAsync(string bucket, Document d)
        {
            AzureTable table = await this.GetTableReference(bucket);
            StoreResponse resp = new StoreResponse { Key = d.Key };
            try
            {
                TableResult result = null;
                IDictionary<String, Object> myEn = new ExpandoObject();
                myEn["RowKey"] = d.Key;
                myEn["PartitionKey"] = bucket;
                myEn["content"] = d.Content;

                foreach (var tagKey in d.Tags)
                {
                    myEn.Add(tagKey);
                }
                if (d.Version == null)
                {
                    result = await table.InsertOrReplaceAsync(myEn);
                }
                else
                {
                    result = await table.ReplaceAsync(myEn, d.Version);
                }
                resp.Version = result.ETag;
                return resp;

            }
            catch (StorageException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
                {
                    throw new ConflictException("There is a document with the same key and another version already stored");
                }
                else
                    throw;
            }
        }

        public async Task DeleteAsync(string bucket, string key, string version)
        {
            AzureTable table = await this.GetTableReference(bucket);
            try
            {
                IDictionary<String, Object> myEn = new ExpandoObject();
                myEn["RowKey"] = key;
                myEn["PartitionKey"] = bucket;
                myEn["soft_deleted"] = true;

                await table.ReplaceAsync(myEn, version);
            }
            catch (StorageException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
                {
                    throw new ConflictException("There is a document with the same key and another version already stored");
                }
                else
                    throw;
            }
        }



        public void Dispose()
        {

        }
    }

}
#endif