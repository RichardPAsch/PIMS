using System;

namespace PIMS.Core.Models.ViewModels
{
    public class AssetSummaryVm
    {
        // View model used in "Asset/Retreive-Quick Edits" menu option which 
        // allows for selected editing while reviewing portfolio assets.
        // "Pre-edit" fields allow for "original data" retreival, before updates
        // are to be applied, and should not be modified by clients.

        public string TickerSymbol { get; set; }
        public string TickerSymbolDescription { get; set; }
        public string AssetClassification { get; set; }
        public string AccountTypePostEdit { get; set; }
        public string AccountTypePreEdit { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string DividendFrequency { get; set; }
        public decimal IncomeRecvd { get; set; }
        //public string DateRecvd { get; set; }
        public DateTime DateRecvd { get; set; }
        public string CurrentInvestor { get; set; }

    }


    public class AssetDetailVm
    {
        public string TickerSymbol { get; set; }
        public string AssetInvestor { get; set; }
        public string AssetClassification { get; set; }
        public string ProfileUrl { get; set; }
        public string PositionsUrl { get; set; }
        public string RevenueUrl { get; set; }

    }
}
