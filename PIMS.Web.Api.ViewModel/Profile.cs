using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PIMS.Web.Api.ViewModel
{
    // SERVICE model type; ViewModel for client.
    public class Profile
    {
        public long ProfileId { get; set; }
        public string DividendFreq { get; set; }
        public decimal? DividendRate { get; set; }
        public decimal DividendYield { get; set; }
        public decimal? PE_Ratio { get; set; }
        public List<Link> Links { get; set; }
    }
}