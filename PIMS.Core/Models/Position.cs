﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace PIMS.Core.Models
{
    public class Position
    {
        // Considered as child object of Aggregate root (Asset).
        public virtual string Url { get; set; }

        // NH relational mapping : 'one' side of Asset
        public virtual Asset PositionAsset { get; set; }

        // NH - composite FK ref.
        public virtual Guid PositionAssetId { get; set; }

        // NH - for PK mapping
        public virtual Guid PositionId { get; set; }

        // NH - 'one' side of Position/AccountType rel.
        public virtual AccountType Account { get; set; }

        // added 4.7.17
        // NH one-side reference for M:1 relationship to Transaction
        public virtual IList<Transaction> PositionTransactions { get; set; } 

        // NH - FK ref. 
        public virtual Guid AcctTypeId { get; set; }

        // NH - mapping for many side of rel: 1:M
        public virtual IList<Income> PositionIncomes { get; set; }  
        

        public virtual string InvestorKey { get; set; }

        // (I)nactive or (A)ctive
        public virtual char Status { get; set; }
        
        //[Required]
        // Date of purchase for Asset.
        public virtual DateTime PurchaseDate { get; set; }

        [Required]
        [Range(0,10000)]
        public virtual decimal Quantity { get; set; }
        
        [Required]
        public virtual DateTime LastUpdate { get; set; }

        public virtual decimal MarketPrice { get; set; }

        public virtual string TickerSymbol { get; set; }

        // TODO: set [Required] attr? If set, will be part of ModelState validation.
        // Date Position added to asset; used in Revenue editing (back-dating checks)
        public virtual DateTime PositionDate { get; set; }

        [Range(0.00, 30000.00)]
        public virtual decimal Fees { get; set; }

        [Range(0.00, 9000.00)]
        public virtual decimal UnitCost { get; set; }



    }
}
