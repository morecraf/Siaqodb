using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SiaqodbCloudService.Models
{
    public class BatchResponse
    {
        public int ItemsWithErrors { get; set; }
        public int Total
        {
            get
            {
                return BatchItemResponses.Count;
            }
        }
        public List<BatchItemResponse> BatchItemResponses { get; set; }
        public string UploadAnchor { get; set; }

    }
    public class BatchItemResponse
    {
        public string Error { get; set; }
        public string ErrorDesc { get; set; }
        public string Version { get; set; }
        public string Key { get; set; }


    }
}