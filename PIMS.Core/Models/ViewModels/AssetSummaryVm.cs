using System;

namespace PIMS.Core.Models.ViewModels
{
    public class AssetSummaryVm
    {
        // View model used in "Asset/Retreive-Quick Edits" Summary menu option which 
        // allows for selected editing while reviewing varied portfolio asset attributes.
        

        // Profile table.
        public string TickerSymbol { get; set; }            //Read-Only
        public string TickerSymbolDescription { get; set; }
        public string DividendFrequency { get; set; }

        // Asset table.
        public string AssetClassification { get; set; }

        //Position table.
        public string AccountTypePostEdit { get; set; }
        public string AccountTypePreEdit { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Income table.
        public decimal IncomeRecvd { get; set; }
        public DateTime DateRecvd { get; set; }

        // TODO: needed?
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
