

using System;

namespace PIMS.Core.Models.ViewModels
{
    // NOTE: Class names closely correspond to Income controller action names.

    public class AssetRevenueVm
    {
        // View model for housing various "Income" querying option results.
        public string Ticker { get; set; }
        public string DateReceived { get; set; }
        public decimal AmountReceived { get; set; }
        public string AccountType { get; set; }
    }

   
    public class TotalRevenueAndPymtFreqByAssetVm
    {
        // View model for housing all asset revenue to date.
        public string Ticker { get; set; }
        public string DatePurchased { get; set; }
        public decimal TotalIncome { get; set; }
        public string IncomeFrequency { get; set; }
    }

    public class RevenueYtdVm
    {
        public int MonthRecvd { get; set; }
        public decimal AmountRecvd { get; set; }
        public decimal YtdAverage { get; set; }
        public decimal Rolling3MonthAverage { get; set; }

        //public decimal TestMethod()
        //{
        //    return 100M;
        //}

    }

    public class AssetProfilesVm
    {
        public string Ticker { get; set; }
        public string TickerDescription { get; set; }
        public decimal DividendYield { get; set; }
        public decimal DividendRate { get; set; }
    }

    public class RevenuePymtFreqVm
    {
        public string TickerSymbol { get; set; }
        public string IncomeFrequency { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal AvgMonthlyRevenue { get; set; }
    }

    public class RevenueLast3MonthsVm
    {
        public int RevenueMonth { get; set; }
        public decimal RevenueAmount { get; set; }
    }

    public class RevenueByDatesVm
    {
        public DateTime BeginningDate { get; set; }
        public DateTime EndingDate { get; set; }
        public decimal RevenueAmount { get; set; }
    }

    public class AssetsRevenueVm
    {
        //public string DateReceived { get; set; }
        public DateTime DateReceived { get; set; }
        public string Ticker { get; set; }
        public string AmountReceived { get; set; }
    }

    public class RevenueByMonthAndDatesVm
    {
        public string YearReceived { get; set; }
        public string MonthReceived { get; set; }
        public decimal TotalReceived { get; set; }
        public int AssetCount { get; set; }
    }

    public class RevenueByFrequencyAndDatesVm
    {
        public string TickerSymbol { get; set; }
        public string RevenueDate { get; set; }
        public string Revenue { get; set; }
        public string Frequency { get; set; }
    }

    public class RevenueByPeriodAndDatesVm<T>
    {
        public string Year { get; set; }
        public T Period { get; set; }
        public string Revenue { get; set; }
    }

    public class ProfileProjectionVm
    {
        public string Ticker { get; set; }
        public decimal Capital { get; set; }
        public decimal Price { get; set; }
        public string DivYield { get; set; }
        public decimal DivRate { get; set; }
        public DateTime DivDate { get; set; }
        public string PE_Ratio { get; set; }
        public decimal ProjectedRevenue { get; set; }
    }
                                                                 
 }


