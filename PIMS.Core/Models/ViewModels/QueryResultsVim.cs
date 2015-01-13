namespace PIMS.Core.Models.ViewModels
{
    // NOTE: Class names closely correspond to Income controller action names.

    public class AssetRevenueForLastTwoYearsVm
    {
        // View model for housing various "Income" querying option results.
        public string Ticker { get; set; }
        public string DateReceived { get; set; }
        public decimal AmountReceived { get; set; }
        
    }

    public class AssetsBasedOnYieldsVm
    {
        // View model for housing Profile dividend data results.
        public string Ticker { get; set; }
        public string TickerDescription { get; set; }
        public decimal DividendYield { get; set; }
        public decimal DividendRate { get; set; }

    }
}
