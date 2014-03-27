using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PIMS.Web.Api.ViewModel
{
    // SERVICE model type; ViewModel for client.
    public class Position
    {
        public decimal MarketPrice { get; set; }
        public long PositionId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalValue { get; set; }
        public decimal UnitPrice { get; set; }
        public List<Link> Links { get; set; }
    }
}