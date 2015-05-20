using System;


namespace PIMS.Core.Models.ViewModels
{
    public class AccountTypeVm
    {
        public Guid KeyId { get; set; }
        public Guid PositionRefId { get; set; }
        public string AccountTypeDesc { get; set; }
        public string Url { get; set; }
    }
}
