using SiaqodbCloudService.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace SiaqodbCloudService.Filters
{
    public class SyncExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            Exception ex = actionExecutedContext.Exception;

            BucketNotFoundException bEx = ex as BucketNotFoundException;
            if (bEx != null)
            {
                actionExecutedContext.Response = HandleBucketNotFoundEx(bEx);
                return;
            }
            DocumentNotFoundException dEx = ex as DocumentNotFoundException;
            if (dEx != null)
            {
                actionExecutedContext.Response = HandleDocNotFoundEx(dEx);
                return;
            }
            InvalidVersionFormatException ivEx = ex as InvalidVersionFormatException;
            if (ivEx != null)
            {
                actionExecutedContext.Response = HandleInvalidVersionEx(ivEx);
                return;
            }
            ConflictException cfEx = ex as ConflictException;
            if (cfEx != null)
            {
                actionExecutedContext.Response = HandleConflictEx(cfEx);
                return;
            }

            actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("An error occurred, please try again or contact the administrator."),
                ReasonPhrase = "internal_error"
            };
        }
        private HttpResponseMessage HandleBucketNotFoundEx(BucketNotFoundException ex)
        {
            var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("Bucket not found!"),
                ReasonPhrase = "bucket_not_found"
            };
            return resp;
        }
        private HttpResponseMessage HandleDocNotFoundEx(DocumentNotFoundException ex)
        {
            var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("Document not found"),
                ReasonPhrase = "document_not_found"
            };
            return resp;
        }
        private HttpResponseMessage HandleInvalidVersionEx(InvalidVersionFormatException ex)
        {
            var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                ReasonPhrase = "version_bad_format"
            };
            return resp;
        }
        private HttpResponseMessage HandleConflictEx(ConflictException ex)
        {
            var resp = new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = new StringContent(ex.Message),
                ReasonPhrase = "conflict"
            };
            return resp;
        }

    }
}