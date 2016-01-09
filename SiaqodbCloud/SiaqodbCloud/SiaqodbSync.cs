using Sqo.Documents;
using Sqo.Documents.Sync;
using Sqo.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbCloud
{
    public class SiaqodbSync : IDisposable
    {
        SiaqodbCloudHttpClient httpClient;
        public SiaqodbSync(string uri, string access_key_id, string secret_access_key)
        {
            if (!Sqo.Internal._bs._hsy())
            {
                throw new SiaqodbException("SiaqodbSync license not valid!");
            }
            this.httpClient = new SiaqodbCloudHttpClient(uri, access_key_id, secret_access_key);
            DownloadBatchSize = 10000;
        }

        public int DownloadBatchSize { get; set; }
#if NON_ASYNC
        public PushResult Push(IBucket bucket)
        {
            return this.Push(bucket, null);
        }
        public PushResult Push(IBucket bucket, IConflictResolver conflictResolver)
        {
            if (!Sqo.Internal._bs._ibs((Bucket)bucket))
            {
                throw new SiaqodbException("Bucket:" + bucket.BucketName + " is not syncable, set it via SiaqodbConfigurator.SetSyncableBucket(...) method");
            }
            var syncStatistics = new PushStatistics();
            syncStatistics.StartTime = DateTime.Now;
            Exception error = null;
            List<Conflict> conflicts = null;
            string uploadAnchor = null;
            try
            {
                this.OnSyncProgress(new SyncProgressEventArgs("Push operation started..."));
                this.OnSyncProgress(new SyncProgressEventArgs("Get local changes..."));

                ChangeSet changeSet = Sqo.Internal._bs._gcs((Bucket)bucket);
                if ((changeSet.ChangedDocuments != null && changeSet.ChangedDocuments.Count > 0) ||
                    (changeSet.DeletedDocuments != null && changeSet.DeletedDocuments.Count > 0))
                {
                    this.OnSyncProgress(new SyncProgressEventArgs("Uploading local changes..."));
                    var response = httpClient.Put(RemovePrefix( bucket.BucketName), changeSet);
                    this.OnSyncProgress(new SyncProgressEventArgs("Upload finished, build the result..."));

                    //update versions
                    var successfullUpdates = response.BatchItemResponses.Where(a => string.IsNullOrEmpty(a.Error))
                        .Select(a=>new KeyValuePair<string,string>(a.Key,a.Version));
                    if (successfullUpdates.Count() > 0)
                    {
                        Sqo.Internal._bs._uv((Bucket)bucket,successfullUpdates);

                        uploadAnchor = response.UploadAnchor;
                    }

                    var conflictResponses = response.BatchItemResponses.Where(a => string.Compare(a.Error, "conflict", StringComparison.OrdinalIgnoreCase) == 0);

                    conflicts = ManageConflicts(bucket, conflictResponses, changeSet, conflictResolver);
                    if (changeSet.ChangedDocuments != null)
                    {
                        syncStatistics.TotalChangesUploads = changeSet.ChangedDocuments.Count;
                    }
                    if (changeSet.DeletedDocuments != null)
                    {
                        syncStatistics.TotalDeletedUploads = changeSet.DeletedDocuments.Count;
                    }
                    if (conflicts != null)
                    {
                        syncStatistics.TotalConflicted = conflicts.Count;
                    }
                    Sqo.Internal._bs._csm((Bucket)bucket);
                }

                this.OnSyncProgress(new SyncProgressEventArgs("Push finshed!"));

            }
            catch (Exception err)
            {
                error = err;
            }
            syncStatistics.EndTime = DateTime.Now;
            return new PushResult(error, syncStatistics, conflicts, uploadAnchor);
        }
        private List<Conflict> ManageConflicts(IBucket bucket, IEnumerable<BatchItemResponse> conflictResponses, ChangeSet changeSet, IConflictResolver conflictResolver)
        {
            List<Conflict> conflicts = null;

            foreach (var conflictR in conflictResponses)
            {
                if (conflicts == null)
                    conflicts = new List<Conflict>();
                var liveVersion = httpClient.Get(RemovePrefix(bucket.BucketName), conflictR.Key);
                var version = liveVersion != null ? liveVersion.Version : null;
                var cf = new Conflict() { Key = conflictR.Key, Version = version, Description = conflictR.ErrorDesc };
                if (conflictResolver == null)
                    conflictResolver = new ServerWinResolver();
                // TODO avoid the extra call to the local database
                Document localVersion = bucket.Load(conflictR.Key);
                KeepChangesByConflictConvension(bucket, localVersion, liveVersion, conflictR.Key, conflictResolver);

                conflicts.Add(cf);
            }
            return conflicts;
        }

        private void KeepChangesByConflictConvension(IBucket bucket, Document localVersion, Document liveVersion, string key, IConflictResolver conflictResolver)
        {
            var winner = conflictResolver.Resolve(localVersion, liveVersion);
            var version = liveVersion != null ? liveVersion.Version : null;
            if (winner == null)//means the winner is the Deleted one
            {
                if (version != null)
                {
                    httpClient.Delete(RemovePrefix(bucket.BucketName), key, version);
                }
                if (localVersion != null)
                {
                   Sqo.Internal._bs._de((Bucket)bucket,key, false);
                }
                return;
            }
            else
            {

                winner.Version = version;
                var resp = httpClient.Put(RemovePrefix(bucket.BucketName), winner);
                winner.Version = resp.Version;
                Sqo.Internal._bs._si((Bucket)bucket,winner,false);
            }
        }


#endif

#if NON_ASYNC
        public PullResult Pull(IBucket bucket, IConflictResolver conflictResolver)
        {
            return this.Pull(bucket, null, conflictResolver);

        }
        public PullResult Pull(IBucket bucket)
        {
            return this.Pull(bucket, null, null);

        }
#endif

#if NON_ASYNC
        public PullResult Pull(IBucket bucket, Filter filter, IConflictResolver conflictResolver = null)
        {
            var pushResult = Push(bucket, conflictResolver);
            var syncStatistics = new PullStatistics();

            if (pushResult.Error != null)
            {
                syncStatistics.StartTime = pushResult.SyncStatistics.StartTime;
                syncStatistics.EndTime = DateTime.Now;
                return new PullResult(pushResult.Error, syncStatistics, pushResult);
            }


            syncStatistics.StartTime = DateTime.Now;
            Exception error = null;
            try
            {
                this.OnSyncProgress(new SyncProgressEventArgs("Pull operation started..."));

                string anchor =Sqo.Internal._bs._gab((Bucket)bucket);

                int remainLimit = 1;
                int nrBatch = 1;
                ChangeSet downloadedItems = null;
                while (remainLimit > 0)
                {
                    remainLimit = 0;
                    this.OnSyncProgress(new SyncProgressEventArgs("Downloading batch #" + nrBatch + " ..."));
                    downloadedItems = DownloadChanges(bucket,filter, anchor, pushResult.UploadAnchor);
                    this.OnSyncProgress(new SyncProgressEventArgs("Batch #" + nrBatch + " downloaded, store items locally ..."));

                    if (downloadedItems != null)
                    {
                        if (downloadedItems.ChangedDocuments != null)
                        {
                            DateTime start = DateTime.Now;
                            Sqo.Internal._bs._sb((Bucket)bucket,downloadedItems.ChangedDocuments, false);
                            string elapsed = (DateTime.Now - start).ToString();
                            remainLimit += downloadedItems.ChangedDocuments.Count;
                            syncStatistics.TotalChangesDownloads += downloadedItems.ChangedDocuments.Count;
                        }
                        if (downloadedItems.DeletedDocuments != null)
                        {
                            foreach (DeletedDocument delObj in downloadedItems.DeletedDocuments)
                            {
                               Sqo.Internal._bs._de((Bucket)bucket,delObj.Key, false);
                            }
                            remainLimit += downloadedItems.DeletedDocuments.Count;
                            syncStatistics.TotalDeletedDownloads += downloadedItems.DeletedDocuments.Count;
                        }
                        anchor = downloadedItems.Anchor;
                        if (!string.IsNullOrEmpty(anchor))
                        {
                            Sqo.Internal._bs._sa((Bucket)bucket,anchor);
                        }
                        this.OnSyncProgress(new SyncProgressEventArgs("Items of batch " + nrBatch + "stored locally ..."));

                    }
                    nrBatch++;
                }
                this.OnSyncProgress(new SyncProgressEventArgs("Push finshed!"));
            }
            catch (Exception ex)
            {
                error = ex;
            }
            syncStatistics.EndTime = DateTime.Now;
            return new PullResult(error, syncStatistics, pushResult);
        }
#endif



#if NON_ASYNC
        private ChangeSet DownloadChanges(IBucket bucket,  Filter query, string anchor,string uploadAnchor)
        {
            ChangeSet changes = null;
            if (query == null)
            {
                changes = httpClient.GetChanges(RemovePrefix(bucket.BucketName), DownloadBatchSize, anchor, uploadAnchor);
            }
            else
            {
                changes = httpClient.GetChanges(RemovePrefix(bucket.BucketName), query, DownloadBatchSize, anchor,uploadAnchor);
            }
            return changes;
        }
#endif


        public event EventHandler<SyncProgressEventArgs> SyncProgress;
        internal void OnSyncProgress(SyncProgressEventArgs args)
        {
            if (this.SyncProgress != null)
            {
                this.SyncProgress(this, args);
            }
        }


        public void Dispose()
        {
            if (httpClient != null)
            {
                httpClient.Dispose();
            }
        }
        private string RemovePrefix(string bucketName)
        {
            if (bucketName.StartsWith("buk_"))
            {
                return bucketName.Remove(0, 4);
            }
            return bucketName;
        }
    }
    public class SyncProgressEventArgs : EventArgs
    {
        public string Message
        {
            get;
            private set;
        }
        public SyncProgressEventArgs(string message)
        {
            this.Message = message;
        }
    }

}
