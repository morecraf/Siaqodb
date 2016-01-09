using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace SiaqodbCloudService.Repository
{
    public class BucketNotFoundException : Exception
    {

        public BucketNotFoundException(string bucketName) : base(bucketName)
        {

        }
    }
    public class DocumentNotFoundException : Exception
    {
        public DocumentNotFoundException(string key, string version) : base(key)
        {
            this.Version = version;
        }
        public string Version { get; private set; }
    }
    public class InvalidVersionFormatException : Exception
    {

    }
    public class ConflictException : Exception
    {

        public ConflictException(string conflict)
            : base(conflict)
        {

        }
    }
    public class GenericCouchDBException : Exception
    {
        HttpStatusCode statusCode;
        public GenericCouchDBException(string error, HttpStatusCode statusCode) : base(error)
        {
            this.statusCode = statusCode;
        }
        public HttpStatusCode StatusCode { get { return statusCode; } }
    }
}