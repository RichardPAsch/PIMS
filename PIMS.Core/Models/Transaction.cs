using System;
using System.ComponentModel.DataAnnotations;


namespace PIMS.Core.Models
{
    public class Transaction
    {
        [Required]
        public virtual Guid TransactionId { get; set; }

        [Required]
        public virtual Guid TransactionPositionId { get; set; }

        // NH - references 'one' side of Position/Transaction relationship.
        public virtual Position TransactionPosition { get; set; }
        
        public virtual string Action { get; set; }

        public virtual decimal Units { get; set; }

        public virtual decimal MktPrice { get; set; }

        public virtual decimal Fees { get; set; }

        public virtual DateTime Date { get; set; }

        public virtual decimal UnitCost { get; set; }

        public virtual decimal CostBasis { get; set; }

        public virtual decimal Valuation { get; set; }


        


    }
}
