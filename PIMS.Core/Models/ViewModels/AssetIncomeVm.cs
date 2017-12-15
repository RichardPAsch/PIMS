using System;
using System.Collections.Generic;


namespace PIMS.Core.Models.ViewModels
{
    /*
        Temporary data receptacle for storing requisite Asset & Position
        data necessary for persisting Income; this reference data is a 
        necessary part of the XLS revenue parsing when recording income.
    */
    public class AssetIncomeVm
    {
        public string RevenueTickerSymbol { get; set; }

        public Guid RevenueAssetId { get; set; }

        public Guid RevenuePositionId { get; set; }

        public string RevenueAccount { get; set; }
    }

}
