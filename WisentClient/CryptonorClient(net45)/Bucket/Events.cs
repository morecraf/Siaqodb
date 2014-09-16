using System;
using System.Collections.Generic;


namespace CryptonorClient
{
    public class PushCompletedEventArgs:EventArgs
    {
         public Exception Error
        {
            get;
            private set;
        }
         public PushStatistics Statistics
        {
            get;
            private set;
        }
        public List<Conflict> Conflicts { get; private set; }
        public PushCompletedEventArgs(Exception error, PushStatistics statistics, List<Conflict> conflicts)
        {
            this.Error = error;
            this.Statistics = statistics;
            this.Conflicts = conflicts;
        }
    }
    public class PullCompletedEventArgs : EventArgs
    {
        public Exception Error
        {
            get;
            private set;
        }
        public PullStatistics Statistics
        {
            get;
            private set;
        }
        public PullCompletedEventArgs(Exception error, PullStatistics statistics)
        {
            this.Error = error;
            this.Statistics = statistics;
          
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

        public int TotalUploads
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
}
