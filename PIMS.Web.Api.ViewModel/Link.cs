using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PIMS.Web.Api.ViewModel
{
    // SERVICE model type; ViewModel for client.
    public class Link
    {
        public string Rel { get; set; }
        public string Href { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
    }
}