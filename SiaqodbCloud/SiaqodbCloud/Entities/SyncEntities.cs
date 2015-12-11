using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbCloud
{
    public class Conflict
    {
        public string Key { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
    }
    public class PushStatistics
    {
        public DateTime StartTime
        {
            get;
            internal set;
        }
        public DateTime EndTime
        {
            get;
            internal set;
        }
        public int TotalUploads
        {
            get
            {
                return this.TotalChangesUploads + this.TotalDeletedUploads;
            }
        }
        public int TotalDeletedUploads
        {
            get;
            internal set;
        }

        public int TotalChangesUploads
        {
            get;
            internal set;
        }
        public int TotalConflicted
        {
            get;
            internal set;
        }


    }
    public class PullStatistics
    {

        public DateTime StartTime
        {
            get;
            internal set;
        }
        public DateTime EndTime
        {
            get;
            internal set;
        }

        public int TotalDownloads
        {
            get
            {
                return this.TotalChangesDownloads + this.TotalDeletedDownloads;
            }
        }
        public int TotalDeletedDownloads
        {
            get;
            internal set;
        }

        public int TotalChangesDownloads
        {
            get;
            internal set;
        }
    }
    public class PushResult
    {
        public Exception Error { get; set; }
        public PushStatistics SyncStatistics { get; set; }

        public List<Conflict> Conflicts { get; set; }
        public string UploadAnchor  { get; set; }
        public PushResult(Exception error, PushStatistics syncStatistics, List<Conflict> conflicts,string uploadAnchor)
        {
            Error = error;
            SyncStatistics = syncStatistics;
            Conflicts = conflicts;
            UploadAnchor = uploadAnchor;
        }
    }
    public class PullResult
    {
        public PullResult(Exception error, PullStatistics syncStatistics, PushResult pushResult)
        {
            Error = error;
            SyncStatistics = syncStatistics;
            PushResult = pushResult;
        }

        public PullStatistics SyncStatistics { get; set; }

        public Exception Error { get; set; }

        public PushResult PushResult { get; set; }

    }
}
