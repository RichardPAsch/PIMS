using System;
using System.ComponentModel.DataAnnotations;


namespace PIMS.Core.Models.ViewModels
{
    public class IncomeVm
    {
        [Required]
        public string AcctType { get; set; }

        [Required]
        [Range(typeof(decimal), "1", "5000")]
        public decimal AmountRecvd { get; set; }

        [Required]
        public DateTime? DateReceived { get; set; }

        public Guid AssetId { get; set; }
        public decimal AmountProjected { get; set; }

        [Required]
        //TODO: RegEx for string DateTimes ?
        //[RegularExpression(@"^\d{1,2}\/\d{1,2}\/\d{4}$")]
        public DateTime? DateUpdated { get; set; }

        public Guid ReferencedPositionId { get; set; }
        public string Url { get; set; }
        
    }
}
