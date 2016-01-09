using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiaqodbCloudService.Models;
using MyCouch;
using SiaqodbCloudService.Repository.CouchDB;
using MyCouch.Requests;
using MyCouch.Responses;

namespace SiaqodbCloudService.Repository
{
    class CouchDBRepo : IRepository
    {
        private const string DbServerUrl = @"http://127.0.0.1:5984/";
        private const string AccessKeysBucket = "sys_accesskeys";
        private const string SyncLogBucket = "sys_synclog";

        public async Task<StoreResponse> Delete(string bucketName, string key, string version)
        {
            using (var client = new MyCouchClient(DbServerUrl, bucketName))
            {
                var startTime = DateTime.Now;
                if (version == null)
                {
                    var response = await client.Documents.GetAsync(key);
                    if (response.IsSuccess)
                    {
                        if (response.Content != null)
                        {
                            CouchDBDocument doc = client.Serializer.Deserialize<CouchDBDocument>(response.Content);
                            version = doc._rev;
                        }

                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        if (response.Reason == "no_db_file")
                            throw new BucketNotFoundException(bucketName);
                        else
                            throw new DocumentNotFoundException(key, version);
                    }
                    else throw new GenericCouchDBException(response.Reason, response.StatusCode);


                }
                var deletedResponse = await client.Documents.DeleteAsync(key, version);
                if (deletedResponse.IsSuccess)
                {
                    return new StoreResponse() { Key = deletedResponse.Id, Version = deletedResponse.Rev };
                }
                else if (deletedResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    if (deletedResponse.Reason == "no_db_file")
                        throw new BucketNotFoundException(bucketName);
                    else
                        throw new DocumentNotFoundException(key, version);
                }
                else if (deletedResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new InvalidVersionFormatException();
                }
                else throw new GenericCouchDBException(deletedResponse.Reason, deletedResponse.StatusCode);


            }
        }

        public async Task<SiaqodbDocument> Get(string bucketName, string key, string version)
        {
            using (var client = new MyCouchClient(DbServerUrl , bucketName))
            {
                var startTime = DateTime.Now;
                var response = await client.Documents.GetAsync(key, version);
                if (response.IsSuccess)
                {
                    var size = response.Content == null ? 0 : response.Content.Length;
                   
                    if (size == 0) return null;
                    var doc = client.Serializer.Deserialize<CouchDBDocument>(response.Content);
                    return Mapper.ToSiaqodbDocument(doc);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    if (response.Reason == "no_db_file")
                        throw new BucketNotFoundException(bucketName);
                    else
                        throw new DocumentNotFoundException(key, version);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new InvalidVersionFormatException();
                }
                else throw new GenericCouchDBException(response.Reason, response.StatusCode);
            }
        }

        public async Task<BatchSet> GetAllChanges(string bucketName, int limit, string anchor,string uploadAnchor)
        {
            using (var client = new MyCouchClient(DbServerUrl, bucketName))
            {
             
                var size = 0;
                GetChangesRequest changesReq = new GetChangesRequest();
                changesReq.Since = anchor;
                changesReq.Limit = limit;
                changesReq.IncludeDocs = true;
                var response = await client.Changes.GetAsync(changesReq);
                if (response.IsSuccess)
                {
                    BatchSet changeSet = new BatchSet();
                    if (response.Results != null)
                    {
                        SyncLogItem logItem = null;
                        if (!string.IsNullOrEmpty(uploadAnchor))
                        {
                            using (var clientLog = new MyCouchClient(DbServerUrl, SyncLogBucket))
                            {
                                logItem = (await clientLog.Entities.GetAsync<SyncLogItem>(uploadAnchor)).Content;
                            }
                        }
                        foreach (var row in response.Results)
                        {
                           
                            if (row.Deleted)
                            {
                                if (changeSet.DeletedDocuments == null)
                                    changeSet.DeletedDocuments = new List<DeletedDocument>();
                                DeletedDocument delObj = new DeletedDocument() { Key = row.Id, Version = row.Changes[0].Rev };
                                //check uploaded anchor- means cliet just uploaded this record and we should not return back
                                if (logItem != null && logItem.KeyVersion != null && logItem.KeyVersion.ContainsKey(delObj.Key) && logItem.KeyVersion[delObj.Key] == delObj.Version)
                                    continue;
                                changeSet.DeletedDocuments.Add(delObj);

                            }
                            else
                            {
                                if (changeSet.ChangedDocuments == null)
                                    changeSet.ChangedDocuments = new List<SiaqodbDocument>();
                                size += row.IncludedDoc.Length;
                                CouchDBDocument co = client.Serializer.Deserialize<CouchDBDocument>(row.IncludedDoc);
                                if (co._id.StartsWith("_design/"))
                                    continue;
                                //check uploaded anchor- means cliet just uploaded this record and we should not return back
                                if (logItem != null && logItem.KeyVersion != null && logItem.KeyVersion.ContainsKey(co._id) && logItem.KeyVersion[co._id] == co._rev)
                                    continue;
                                changeSet.ChangedDocuments.Add(Mapper.ToSiaqodbDocument(co));
                            }
                        }
                        changeSet.Anchor = response.LastSeq;
                    }
                    int nrChangedObjs = 0;
                    int nrDeletedObjs = 0;
                    if (changeSet.ChangedDocuments != null)
                        nrChangedObjs = changeSet.ChangedDocuments.Count;
                    if (changeSet.DeletedDocuments != null)
                        nrDeletedObjs = changeSet.DeletedDocuments.Count;

                    return changeSet;
                }
                else CheckBucketNotFound(bucketName, response);
                return null;

            }
        }
        private static void CheckBucketNotFound(string bucketName, Response response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound && response.Reason == "no_db_file")
            {
                throw new BucketNotFoundException(bucketName);
            }
            else throw new GenericCouchDBException(response.Reason, response.StatusCode);
        }

        public async Task<BatchSet> GetChanges(string bucketName, Filter query, int limit, string anchor,string uploadAnchor)
        {
            using (var client = new MyCouchClient(DbServerUrl, bucketName))
            {
                // loggingService.BeforeActionLog(bucketName, "GetChanges", value, anchor);
                var startTime = DateTime.Now;
                GetChangesRequest changesReq = new GetChangesRequest();
                changesReq.Since = anchor;
                changesReq.Limit = limit;
                changesReq.IncludeDocs = false;
                var response = await client.Changes.GetAsync(changesReq);
                if (response.IsSuccess)
                {
                    BatchSet changeSet = new BatchSet();
                    if (response.Results != null)
                    {
                        HashSet<string> changesHashSet = new HashSet<string>();
                        SyncLogItem logItem = null;
                        if (!string.IsNullOrEmpty(uploadAnchor))
                        {
                            using (var clientLog = new MyCouchClient(DbServerUrl, SyncLogBucket))
                            {
                                logItem = (await clientLog.Entities.GetAsync<SyncLogItem>(uploadAnchor)).Content;
                            }
                        }
                        foreach (var row in response.Results)
                        {
                            if (row.Deleted && !string.IsNullOrEmpty(anchor))
                            {
                                if (changeSet.DeletedDocuments == null)
                                    changeSet.DeletedDocuments = new List<DeletedDocument>();
                            
                                DeletedDocument delObj = new DeletedDocument() { Key = row.Id, Version = row.Changes[0].Rev };
                                //check uploaded anchor- means cliet just uploaded this record and we should not return back
                                if (logItem != null && logItem.KeyVersion != null && logItem.KeyVersion.ContainsKey(delObj.Key) && logItem.KeyVersion[delObj.Key] == delObj.Version)
                                    continue;
                                changeSet.DeletedDocuments.Add(delObj);

                            }
                            else
                            {
                                changesHashSet.Add(row.Id);
                            }
                        }
                        changeSet.Anchor = response.LastSeq;
                        HashSet<string> changesHashSetOfQuery = await GetDocIdsByQuery(client, query);

                        HashSet<string> docsIds = Intersect(changesHashSet, changesHashSetOfQuery);
                        if (docsIds.Count > 0)
                        {
                            int i = 0;
                            changeSet.ChangedDocuments = new List<SiaqodbDocument>();
                            List<string> docsPart = new List<string>();
                            foreach (string docId in docsIds)
                            {
                                docsPart.Add(docId);
                                if (i % 100 == 0 && i > 0)
                                {
                                    var resultSet = await this.GetByTag(client, PrepareQueryIN(docsPart));
                                    foreach (SiaqodbDocument document in resultSet)
                                    {
                                        //check uploaded anchor- means cliet just uploaded this record and we should not return back
                                        if (logItem != null && logItem.KeyVersion != null && logItem.KeyVersion.ContainsKey(document.Key) && logItem.KeyVersion[document.Key] == document.Version)
                                            continue;
                                        ((List<SiaqodbDocument>)changeSet.ChangedDocuments).Add(document);
                                    }

                                    docsPart = new List<string>();
                                }
                                i++;
                            }
                            if (docsPart.Count > 0)
                            {
                                var resultSet = await this.GetByTag(client, PrepareQueryIN(docsPart));

                                foreach (SiaqodbDocument document in resultSet)
                                {
                                    //check uploaded anchor- means cliet just uploaded this record and we should not return back
                                    if (logItem != null && logItem.KeyVersion != null && logItem.KeyVersion.ContainsKey(document.Key) && logItem.KeyVersion[document.Key] == document.Version)
                                        continue;
                                    ((List<SiaqodbDocument>)changeSet.ChangedDocuments).Add(document);
                                }


                            }
                        }

                    }                   
                    return changeSet;
                }
                else CheckBucketNotFound(bucketName, response);
                return null;

            }
        }
        private async Task<List<SiaqodbDocument>> GetByTag(MyCouchClient client, Filter value, string bucketName = "")
        {
           
            var nrViews = 0;
            bool isKey = false;
            if (String.Compare(value.TagName, "key", StringComparison.OrdinalIgnoreCase) == 0)
            {
                isKey = true;
                var viewsInfo = await CheckViewsAndReturnNumber(client.Connection.DbName, client);
                nrViews = viewsInfo.Item2;
                SkipDesignDocs(value, viewsInfo);
            }
            var query = PrepareQuery(value);
            query.IncludeDocs = true;

            var size = 0;
           
            var response = await client.Views.QueryAsync(query);
            if (response.IsSuccess)
            {
               

                List<SiaqodbDocument> list = new List<SiaqodbDocument>();
                if (response.Rows != null)
                {
                    foreach (var row in response.Rows)
                    {
                        if (row.Id != null && row.IncludedDoc != null)
                        {
                            size += row.IncludedDoc.Length;
                            CouchDBDocument co = client.Serializer.Deserialize<CouchDBDocument>(row.IncludedDoc);
                            if (isKey && co._id.StartsWith("_design/"))
                                continue;
                            list.Add(Mapper.ToSiaqodbDocument(co));
                        }
                    }
                }
                             
                return list;
            }
            else if (response.Reason == "missing" && response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                //tag not found
                return new List<SiaqodbDocument>();
            }
            else CheckBucketNotFound(bucketName, response);
            return null;
        }
        private void SkipDesignDocs(Filter value, Tuple<long, int> viewsInfo)
        {
            if (!value.Skip.HasValue && !value.Limit.HasValue)//from Sync/GetChanges
            {
                return;
            }
            int skip = value.Skip == null ? 0 : value.Skip.Value;
            int limit = value.Limit == null ? 0 : value.Limit.Value;
            SkipDesignDocs(ref skip, ref limit, viewsInfo);
            value.Skip = skip;
            value.Limit = limit;
        }
        private void SkipDesignDocs(ref int skip, ref int limit, Tuple<long, int> viewsInfo)
        {
            if (skip + limit > viewsInfo.Item1
                && skip <= viewsInfo.Item1)
                limit += viewsInfo.Item2;

            if (skip > viewsInfo.Item1)
                skip += viewsInfo.Item2;
        }
        /// <summary>
        /// TODO improve this and do not check for every query
        /// Problem is Offset of _design/viewName which can change after every insert
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        private async Task<Tuple<long, int>> CheckViewsAndReturnNumber(string bucketName, MyCouchClient client)
        {
            long offset = 0;
            int nrViews = 0;

            QueryViewRequest query = new QueryViewRequest("_all_docs");
            query.StartKey = "_design/";
            query.EndKey = "_design0";

            var all = await client.Views.QueryAsync(query);
            if (!all.IsSuccess)
                CheckBucketNotFound(bucketName, all);

            offset = all.OffSet;
            nrViews = all.Rows != null ? all.Rows.Count() : 0;

            return new Tuple<long, int>(offset, nrViews);
        }
        private async Task<HashSet<string>> GetDocIdsByQuery(MyCouchClient client, Filter value)
        {
            HashSet<string> changesHashSetOfQuery = new HashSet<string>();
            var couchQuery = PrepareQuery(value);

            DateTime start = DateTime.Now;
            var responseQuery = await client.Views.QueryAsync(couchQuery);
            if (responseQuery.IsSuccess)
            {
                if (responseQuery.Rows != null)
                {
                    foreach (var row in responseQuery.Rows)
                    {
                        if (row.Id != null)
                        {
                            changesHashSetOfQuery.Add(row.Id);
                        }
                    }
                }
            }

            return changesHashSetOfQuery;
        }
        private QueryViewRequest PrepareQuery(Filter value)
        {
            string viewName = "tags_" + value.TagName;
            QueryViewRequest query = null;
            if (string.Compare(value.TagName, "key", true) == 0)
            {
                query = new QueryViewRequest("_all_docs");
            }
            else
            {
                query = new QueryViewRequest(viewName, viewName);
            }
            query.StartKey = value.Start;
            query.EndKey = value.End;
            query.Key = value.Value;
            query.Limit = value.Limit;
            query.Skip = value.Skip;
            query.Keys = value.In;
            query.Descending = value.Descending;
            return query;
        }
        private HashSet<string> Intersect(HashSet<string> hashSet1, HashSet<string> hashSet2)
        {
            if (hashSet1.Count < hashSet2.Count)
            {
                hashSet1.IntersectWith(hashSet2);
                return hashSet1;

            }
            else
            {
                hashSet2.IntersectWith(hashSet1);
                return hashSet2;

            }
        }
        private Filter PrepareQueryIN(List<string> ids)
        {
            Filter queryFinal = new Filter();
            queryFinal.In = ids.ToArray();
            queryFinal.TagName = "key";
            return queryFinal;
        }
        public async Task<string> GetSecretAccessKey(string accessKeyId)
        {
            using (var client = new MyCouchClient(DbServerUrl, AccessKeysBucket))
            {
                var response = await client.Documents.GetAsync(accessKeyId);
                if (response.IsSuccess)
                {
                    var size = response.Content == null ? 0 : response.Content.Length;

                    if (size == 0) return null;
                    var doc = client.Serializer.Deserialize<AccessKey>(response.Content);
                    return doc.secretkey;
                }
                
            }
            return null;
        }

        public async Task<StoreResponse> Store(string bucketName, SiaqodbDocument document)
        {
            using (var client = new MyCouchClient(DbServerUrl, bucketName))
            {
                
                await CheckTagsViews(client, bucketName, document.Tags);
                CouchDBDocument doc = Mapper.ToCouchDBDoc(document);
                var serializedObj = client.Serializer.Serialize<CouchDBDocument>(doc);

                var response = await client.Documents.PostAsync(serializedObj);
                if (response.IsSuccess)
                {
                    var cnorResponse = new StoreResponse();
                    cnorResponse.Version = response.Rev;
                    cnorResponse.Key = response.Id;

                    await this.StartRebuildViews(client, document);
                    
                    return cnorResponse;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new BucketNotFoundException(bucketName);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    throw new ConflictException(response.Reason);
                }
                else throw new GenericCouchDBException(response.Reason, response.StatusCode);
            }
        }

        public async Task<BatchResponse> Store(string bucketName, BatchSet batch)
        {
            using (var client = new MyCouchClient(DbServerUrl, bucketName))
            {
               
                BulkRequest bulkRequest = new BulkRequest();
               
                DateTime start = DateTime.Now;
                int size = 0;
                SiaqodbDocument crObjForUpdateViews = null;
                if (batch.ChangedDocuments != null)
                {
                    foreach (SiaqodbDocument obj in batch.ChangedDocuments)
                    {
                        if (obj != null)
                        {
                            if (crObjForUpdateViews == null)
                                crObjForUpdateViews = obj;

                            await CheckTagsViews(client, bucketName, obj.Tags);
                            CouchDBDocument doc = Mapper.ToCouchDBDoc(obj);
                            var serializedObject = client.Serializer.Serialize<CouchDBDocument>(doc);
                            bulkRequest.Include(serializedObject);
                            size += serializedObject.Length;
                        }
                    }
                }
                if (batch.DeletedDocuments != null)
                {
                    foreach (DeletedDocument obj in batch.DeletedDocuments)
                    {
                        if (obj != null)
                        {
                            if (obj.Version != null)//otherwise means is a non-existing object
                            {
                                bulkRequest.Delete(obj.Key, obj.Version);
                            }
                        }

                    }
                }
                var response = await client.Documents.BulkAsync(bulkRequest);
                if (response.IsSuccess)
                {
                    var cnorResponse = new BatchResponse();
                    if (response.Rows != null)
                    {
                        cnorResponse.BatchItemResponses = new List<BatchItemResponse>();
                        SyncLogItem syncLogItem = new SyncLogItem();
                        syncLogItem.KeyVersion = new Dictionary<string, string>();
                        foreach (var row in response.Rows)
                        {
                            BatchItemResponse wresp = new BatchItemResponse();
                            if (!string.IsNullOrEmpty(row.Error))
                            {
                                cnorResponse.ItemsWithErrors++;
                            }
                            wresp.Error = row.Error;
                            wresp.ErrorDesc = row.Reason;
                            wresp.Key = row.Id;
                            wresp.Version = row.Rev;
                            cnorResponse.BatchItemResponses.Add(wresp);
                            if (string.IsNullOrEmpty(row.Error))
                            {
                                syncLogItem.KeyVersion.Add(row.Id, row.Rev);
                            }
                        }
                        if (syncLogItem.KeyVersion.Count > 0)
                        {
                            syncLogItem.TimeInserted = DateTime.UtcNow;
                            using (var clientLog = new MyCouchClient(DbServerUrl, SyncLogBucket))
                            {
                                var logResp = await clientLog.Entities.PostAsync(syncLogItem);
                                cnorResponse.UploadAnchor = logResp.Id;
                            }
                        }
                    }
                    if (crObjForUpdateViews != null)
                    {
                        await this.StartRebuildViews(client, crObjForUpdateViews);
                    }
                    
                    return cnorResponse;
                }
                else CheckBucketNotFound(bucketName, response);
                return null;

            }
        }
        private async Task StartRebuildViews(MyCouchClient client, SiaqodbDocument crObjForUpdateViews)
        {
            //force building the index at Store time avoiding to wait on Query time
            if (crObjForUpdateViews.Tags != null)
            {
                foreach (string tagName in crObjForUpdateViews.Tags.Keys)
                {
                    string viewName = "tags_" + tagName;
                    QueryViewRequest query = new QueryViewRequest(viewName, viewName);
                    query.Stale = Stale.UpdateAfter;
                    query.Limit = 1;
                    var res = await client.Views.QueryAsync(query);

                }
            }
        }
        private async Task CheckTagsViews(MyCouchClient client, string bucketName, Dictionary<string, object> tags)
        {
            if (tags != null && tags.Count > 0)
            {
                HashSet<string> viewsCache = new HashSet<string>();


                QueryViewRequest query = new QueryViewRequest("_all_docs");
                query.StartKey = "_design/";
                query.EndKey = "_design0";

                var all =await client.Views.QueryAsync(query);
                if (!all.IsSuccess)
                    CheckBucketNotFound(bucketName, all);
                if (all.Rows != null)
                {
                    foreach (var row in all.Rows)
                    {
                        string viewName = row.Key.ToString().Replace("_design/", "");
                        viewsCache.Add(viewName);

                    }
                }


                foreach (string tagName in tags.Keys)
                {
                    string viewName = "tags_" + tagName;
                    if (!viewsCache.Contains(viewName) && tags[tagName] != null)
                    {

                        string viewJSON = @"{""_id"":""_design/" + viewName + @""",""language"":""javascript"",""views"":{""" + viewName + @""":{""map"":
                            ""function(doc) {if(doc.tags." + tagName + @"!=null)emit(doc.tags." + tagName + @", null);}""}}}";
                        var getJson = await client.Documents.PostAsync(viewJSON);
                        if (!getJson.IsSuccess)
                            CheckBucketNotFound(bucketName, getJson);
                        viewsCache.Add(viewName);



                    }
                }

            }


        }
    }
}
