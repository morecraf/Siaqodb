using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SiaqodbCloudService.Repository.CouchDB
{
    public class CouchDBDocument
    {
        public string _id { get; set; }
        public string _rev { get; set; }
        public byte[] doc { get; set; }
        public Dictionary<string, object> tags { get; set; }
    }
    public class AccessKey
    {
        public string _id { get; set; }
        public string _rev { get; set; }
        public string secretkey { get; set; }
    }
}