using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;


namespace PIMS.Core.Models.ViewModels
{
    public class PositionEditsVm
    {
        [Required]
        public Guid FromPosId { get; set; }

        
        public Guid ToPosId { get; set; }

        [Required]
        public Guid PositionAssetId { get; set; }

        [Required]
        public Guid PositionFromAccountId { get; set; }

        [Required]
        public Guid PositionToAccountId { get; set; }

        [Required]
        public string FromPositionStatus { get; set; }

        public string ToPositionStatus { get; set; }

        [Required]
        public int FromQty { get; set; }

        public int ToQty { get; set; }

        [Required]
        public decimal FromUnitCost { get; set; }

        public decimal ToUnitCost { get; set; }

        [Required]
        public DateTime LastUpdate { get; set; }

        [Required]
        public Guid PositionInvestorId { get; set; }

        [Required]
        public DateTime FromPositionDate { get; set; }

        public DateTime ToPositionDate { get; set; }

        [Required]
        public string DbActionOrig { get; set; }

        [Required]
        public string DbActionNew { get; set; }
        
        [Required]
        public DateTime FromPurchaseDate { get; set; }

        public DateTime ToPurchaseDate { get; set; }

    }
}
