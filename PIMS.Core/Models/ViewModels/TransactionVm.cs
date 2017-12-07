using System;


namespace PIMS.Core.Models.ViewModels
{
    public class TransactionVm
    {
        public Guid PositionId { get; set; }
        public Guid TransactionId { get; set; }
        public string TransactionEvent { get; set; } // e.g. 'B','S'
        public int Units { get; set; }
        public decimal MktPrice { get; set; }
        public decimal Fees { get; set; }
        public decimal UnitCost { get; set; }
        public  decimal CostBasis { get; set; }
        public decimal Valuation { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DatePositionCreated { get; set; }
        public string TickerSymbol { get; set; }
    }

}
