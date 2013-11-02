using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbSyncMobile
{
    public class SyncProgressEventArgs : EventArgs
    {

        public SyncProgressEventArgs(string message)
        {
            this.Message = message;
        }
        public string Message { get; private set; }
    }
    public class SyncCompletedEventArgs : EventArgs
    {
        public SyncCompletedEventArgs(Exception error, SyncStatistics statistics)
        {
            
            this.Error = error;
            this.Statistics = statistics;
        }
       
        public Exception Error { get; private set; }
        public SyncStatistics Statistics { get; private set; }
    }
    /// <summary>
    /// Class that represents the stats for a sync session.
    /// </summary>
    public class SyncStatistics
    {
        /// <summary>
        /// Start Time of Sync Session
        /// </summary>
        public DateTime StartTime { get; internal set; }

        /// <summary>
        /// End Time of Sync Session
        /// </summary>
        public DateTime EndTime { get; internal set; }

        /// <summary>
        /// Total number of Uploded Items
        /// </summary>
        public uint TotalUploads
        {
            get
            {
                return TotalInsertedUploads + TotalUpdatedUploads + TotalDeletedUploads;
            }
        }

        /// <summary>
        /// Total number of Deleted Uploded Items
        /// </summary>
        public uint TotalDeletedUploads { get; internal set; }

        /// <summary>
        /// Total number of Updated Uploded Items
        /// </summary>
        public uint TotalUpdatedUploads { get; internal set; }

        /// <summary>
        /// Total number of Updated Uploded Items
        /// </summary>
        public uint TotalInsertedUploads { get; internal set; }

        /// <summary>
        /// Total number of downloaded items
        /// </summary>
        public uint TotalDownloads { get; internal set; }

        
    }
}
