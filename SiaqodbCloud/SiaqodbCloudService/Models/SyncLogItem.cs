using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SiaqodbCloudService.Models
{
    public class SyncLogItem
    {
        public string Id { get; set; }
        public string Rev { get; set; }
        public Dictionary<string, string> KeyVersion { get; set; }
        public DateTime TimeInserted { get; set; }
    }
}