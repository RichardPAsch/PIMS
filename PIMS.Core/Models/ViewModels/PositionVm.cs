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
        public decimal Qty { get; set; }

        [Required]
        public decimal UnitCost { get; set; }

        [Required]
        public DateTime? DateOfPurchase { get; set; }

        [Required]
        public DateTime? LastUpdate { get; set; }

        public string Url { get; set; }

        public string LoggedInInvestor { get; set; }

        // TODO: 5.13.16 - Shouldn't this be of type atring? Or is it really needed?
        public AccountTypeVm ReferencedAccount { get; set; }

        public TransactionVm ReferencedTransaction { get; set; }

        public Guid CreatedPositionId { get; set; }

        public Guid ReferencedAssetId { get; set; }

        public string ReferencedTickerSymbol { get; set; }

        public DateTime? DatePositionAdded { get; set; }

        public string Status { get; set; }

        // TODO: 7.12.17 - possibly obsolete? use Transaction.Fees instead ?
        public decimal TransactionFees { get; set; }
    }
    

}
