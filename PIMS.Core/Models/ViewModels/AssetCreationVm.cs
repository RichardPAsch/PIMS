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
        
        public ProfileVm ProfileToCreate { get; set; }
        
        
        public PositionVm PositionToCreate { get; set; }                // includes AccountType

        [Required]                                                      // added 5.10.16 for model state validation check
        public List<PositionVm> PositionsCreated { get; set; }

        public string AssetIdentification { get; set; }                 // Map -> Position Asset ref.
 

        public IncomeVm IncomeToCreate { get; set; }

        public List<IncomeVm> RevenueCreated { get; set; }              // Optional



    }
}
