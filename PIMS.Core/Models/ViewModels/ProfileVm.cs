using System;


namespace PIMS.Core.Models.ViewModels
{
    public class ProfileVm
    {
        public Guid ProfileId { get; set; }
        public string TickerSymbol { get; set; }
        public string TickerDescription { get; set; }
        public string DividendFreq { get; set; }
        public decimal DividendRate { get; set; }
        public decimal? DividendYield { get; set; }
        public decimal EarningsPerShare { get; set; }
        // ReSharper disable once InconsistentNaming
        public decimal? PE_Ratio { get; set; }
        public DateTime? LastUpdate { get; set; }
        public DateTime? ExDividendDate { get; set; }
        public DateTime? DividendPayDate { get; set; }
        public decimal Price { get; set; }
        public string Url { get; set; }
   }
}

