using SiaqodbCloudService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbCloudService.Repository.CouchDB
{
    public static class Mapper
    {

        public static CouchDBDocument ToCouchDBDoc(SiaqodbDocument cobj)
        {
            var doc = new CouchDBDocument() { _id = cobj.Key, doc = cobj.Content, tags = cobj.Tags };
            if (cobj.Version == string.Empty)
                doc._rev = null;
            else doc._rev = cobj.Version;
            return doc;
        }
        public static SiaqodbDocument ToSiaqodbDocument(CouchDBDocument cobj)
        {
            return new SiaqodbDocument() { Key = cobj._id, Version = cobj._rev, Content = cobj.doc, Tags = cobj.tags };
        }
    }
}
