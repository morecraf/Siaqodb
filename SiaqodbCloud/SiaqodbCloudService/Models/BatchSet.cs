using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbCloudService.Models
{
    public class BatchSet
    {
        public IList<SiaqodbDocument> ChangedDocuments { get; set; }
        public IList<DeletedDocument> DeletedDocuments { get; set; }
        public string Anchor { get; set; }

    }
    public class DeletedDocument
    {
        public string Key { get; set; }
        public string Version { get; set; }
    }
}
