using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Synchronization.ClientServices;

namespace SiaqodbSyncProvider
{
    public class SyncProgressEventArgs:EventArgs
    {
        
        public SyncProgressEventArgs(string message)
        {
            this.Message = message;
        }
        public string Message { get; private set; }
    }
    public class SyncCompletedEventArgs : EventArgs
    {
        public SyncCompletedEventArgs(bool cancelled,Exception error,CacheRefreshStatistics statistics)
        {
            this.Cancelled = cancelled;
            this.Error = error;
            this.Statistics = statistics;
        }
        public bool Cancelled { get; private set; }
        public Exception  Error { get; private set; }
        public CacheRefreshStatistics Statistics { get; private set; }
    }
}
