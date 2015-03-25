using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PIMS.Core.Interfaces;


namespace PIMS.Core.Models
{
    /* Property names MUST match table column names, IF not noted otherwise in mapping classes. */

    public class AccountType : IEntity
    {
        // NH PK Mapping: AccountTypeId 
        [Key]
        public virtual Guid KeyId { get; set; }

        [Required]
        public virtual Guid PositionRefId { get; set; }  // initialized during Position creation

        // NH 'many' side of 1:M Position/AccountType relationship mapping requirement.
        public virtual IList<Position> Positions { get; set; }

        [Required]
        public virtual string AccountTypeDesc { get; set; }
        

        public virtual string Url { get; set; }
        
       
    }
}
