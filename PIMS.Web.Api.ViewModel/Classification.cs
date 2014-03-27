using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PIMS.Web.Api.ViewModel
{
    // SERVICE model type; ViewModel for client.
    public class Classification
    {
        public long ClassificationId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int Ordinal { get; set; }
        public List<Link> Links { get; set; }
    }
}