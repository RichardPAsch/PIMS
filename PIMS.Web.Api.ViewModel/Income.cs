using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PIMS.Web.Api.ViewModel
{
    // SERVICE model type; ViewModel for client.
    public class Income
    {
        public long IncomeId { get; set; }
        public decimal Actual { get; set; }
        public decimal Projected { get; set; }
        public DateTime? DateRecvd { get; set; }
        public List<Link> Links { get; set; }
    }
}