using Newtonsoft.Json;
using Sqo.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbManager.DocSerializer
{
    public class DocumentWrapper
    {
        public string Key { get; set; }
        public dynamic Content { get; set; }
        public Dictionary<string, object> Tags { get; set; }
        public DocumentWrapper(Document document)
        {
            this.Key = document.Key;
            this.Content= JsonConvert.DeserializeObject<dynamic>(Encoding.UTF8.GetString(document.Content));
            this.Tags = document.Tags;
        }
        public DocumentWrapper()
        {

        }
    }
}
