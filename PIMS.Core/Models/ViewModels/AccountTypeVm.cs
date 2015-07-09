using System;
using System.ComponentModel.DataAnnotations;


namespace PIMS.Core.Models.ViewModels
{
    public class AccountTypeVm
    {
        public Guid KeyId { get; set; }
        public Guid PositionRefId { get; set; }

        [Required]
        public string AccountTypeDesc { get; set; }

        public string Url { get; set; }
    }
}
