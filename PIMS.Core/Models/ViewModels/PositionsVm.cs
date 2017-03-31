using System;
using System.ComponentModel.DataAnnotations;


namespace PIMS.Core.Models.ViewModels
{
    public class PositionsVm
    {
        [Required]
        public string PositionAccountType { get; set; }

        public string PositionTickerSymbol { get; set; }

        public Guid PositionAssetId { get; set; }

        public DateTime PositionAddDate { get; set; }

        public Guid PositionId { get; set; }

        public Guid PositionAccountTypeId { get; set; }

        public Guid PositionInvestorId { get; set; }

        public decimal PositionFees { get; set; }

    }



}