using System;
using System.ComponentModel.DataAnnotations;
using PIMS.Core.Interfaces;

namespace PIMS.Core.Models
{
    /* Property names MUST match table column names, IF not noted otherwise in mapping classes. */

    public class AccountType : IEntity
    {
        public virtual string Url { get; set; }

        [Key]
        public virtual Guid KeyId { get; set; }  // Mapping: AccountTypeId

       
        [Required]
        public virtual string AccountTypeDesc { get; set; }

    }
}
