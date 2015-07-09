using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace PIMS.Core.Models.ViewModels
{
    public class AssetCreationVm
    {

        public string AssetInvestorId { get; set; }                     // Map -> Asset
      
        [Required]
        public string AssetTicker { get; set; }                         // Map -> Profile

        [Required]
        public string AssetDescription { get; set; }                    // Map -> Profile

        [Required]
        public string AssetClassification { get; set; }                 // Map -> AssetClass; recvd from client
        public string AssetClassificationId { get; set; }               // Map -> Asset; persist to Db
        

        //TODO: Set additional "[Required]" attributes as needed via UI testing.
        public string AssetProfileId { get; set; }                      // Map -> Asset; persist to Db
        [Required]
        public decimal PricePerShare { get; set; }                      // Map -> Profile
        public decimal DividendPerShare { get; set; }                   // Map -> Profile
        [Required]
        public decimal DividendYield { get; set; }                      // Map -> Profile
        public decimal PriceEarningsRatio { get; set; }                 // Map -> Profile
        public decimal EarningsPerShare { get; set; }                   // Map -> Profile
        public DateTime? ExDividendDate { get; set; }                   // Map -> Profile
        public DateTime? DividendPayDate { get; set; }                  // Map -> Profile
        public string DividendFrequency { get; set; }                   // Map -> Profile


        public PositionVm PositionToCreate { get; set; }          // includes AccountType
        public List<PositionVm> PositionsCreated { get; set; }
        public string AssetIdentification { get; set; }                 // Map -> Position Asset ref.
        //public AccountTypeVm AssociatedAccount { get; set; }            // Map -> AccountType


        public IncomeVm IncomeToCreate { get; set; }
        public List<IncomeVm> RevenueCreated { get; set; }              // Optional



    }
}
