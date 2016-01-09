using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SiaqodbCloudService.Models
{
    public class SiaqodbDocument
    {
        public string Key { get; set; }
        public string Version { get; set; }
        public byte[] Content { get; set; }

        private Dictionary<string, object> tags;

        public Dictionary<string, object> Tags
        {
            get
            {
                return tags;
            }

            set { tags = value; }

        }

    }
}