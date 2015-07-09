﻿using System;
using System.ComponentModel.DataAnnotations;


namespace PIMS.Core.Models.ViewModels
{
    public class PositionVm
    {
        [Required]
        public string PreEditPositionAccount { get; set; }

        public string PostEditPositionAccount { get; set; }

        [Required]
        public int Qty { get; set; }

        [Required]
        public decimal CostBasis { get; set; }

        [Required]
        public DateTime? DateOfPurchase { get; set; }

        [Required]
        public DateTime? LastUpdate { get; set; }

        public string Url { get; set; }

        public string LoggedInInvestor { get; set; }

        public AccountTypeVm ReferencedAccount { get; set; }

        public Guid CreatedPositionId { get; set; }

        public Guid ReferencedAssetId { get; set; }

        public string ReferencedTickerSymbol { get; set; }
    }
}
