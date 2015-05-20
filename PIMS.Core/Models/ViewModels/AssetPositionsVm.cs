

namespace PIMS.Core.Models.ViewModels
{
    public class AssetPositionsVm
    {
  
        public string PreEditPositionAccount { get; set; }
        public string PostEditPositionAccount { get; set; }
        public int Qty { get; set; }
        public decimal MarketPrice { get; set; } 
        public string DateOfPurchase { get; set; }
        public string LastUpdate { get; set; }
        public string Url { get; set; }
        public string LoggedInInvestor { get; set; }
    }
}
