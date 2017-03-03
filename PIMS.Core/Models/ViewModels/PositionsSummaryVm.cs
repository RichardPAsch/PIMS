
namespace PIMS.Core.Models.ViewModels
{
    public class PositionsSummaryVm
    {
        public string PositionSummaryTickerSymbol { get; set; }

        public string PositionSummaryAccountType { get; set; }

        public int PositionSummaryQty { get; set; }

        public decimal PositionSummaryGainLoss { get; set; }

        public decimal PositionSummaryValuation { get; set; }
        
    }



}
