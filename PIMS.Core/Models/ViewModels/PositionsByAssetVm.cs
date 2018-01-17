using System;
using System.ComponentModel.DataAnnotations;


// Usage: HttpGet via GetPositionsByAssetForEdits(string tickerSymbol)
namespace PIMS.Core.Models.ViewModels
{
    public class PositionsByAssetVm
    {
        [Required]
        public string PreEditPositionAccount { get; set; }

        [Required]
        public decimal Qty { get; set; }

        [Required]
        public decimal UnitCost { get; set; }

        [Required]
        public DateTime? DateOfPurchase { get; set; }

        [Required]
        public DateTime? LastUpdate { get; set; }

        [Required]
        public string ReferencedTickerSymbol { get; set; }

        [Required]
        public DateTime? DatePositionAdded { get; set; }


        public Guid PositionId { get; set; }

    }
}
