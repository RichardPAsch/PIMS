using System.ComponentModel.DataAnnotations;


namespace PIMS.Core.Models.ViewModels
{
    public class PositionsVm
    {
        [Required]
        public string PositionAccountType { get; set; }

        public string PositionTickerSymbol { get; set; }

    }



}