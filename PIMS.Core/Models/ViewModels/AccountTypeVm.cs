using System;
using System.ComponentModel.DataAnnotations;


namespace PIMS.Core.Models.ViewModels
{
    public class AccountTypeVm
    {
        public Guid KeyId { get; set; }

        // 2.29.16 - This vm should only serve as a lookup container for AccountType, need to re-evaluate need for PositionRefId, as Position table 
        //           now rferences assets & positions?
        //public Guid PositionRefId { get; set; }

        [Required]
        public string AccountTypeDesc { get; set; }

        // 2.29.16 - Keep for now, as used by MapVmToAccountType(). 
        public string Url { get; set; }
        
    }
}
