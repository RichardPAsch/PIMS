using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace PIMS.Web.Api.ViewModel
{
    // SERVICE model type; ViewModel for client.
    public class Asset
    {
        public long AssetId { get; set; }
        public List<Classification> Classification {get; set;}
        public Income Income { get; set; }
        public Position Position { get; set; }
        public Profile Profile { get; set; }
        public string TickerSymbol { get; set; }
        public List<Link> Links { get; set; }
        public User User { get; set; }
    }
}