using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace PIMS.Core.Models
{
    public class Investor 
    {
        public virtual string Url { get; set; }

        [Required]
        public virtual Guid InvestorId { get; set; }

        [Required]
        public virtual Guid AspNetUsersId { get; set; }

        [Required]
        public virtual  string LastName { get; set; }

        [Required]
        public virtual  string FirstName { get; set; }

        public virtual string MiddleInitial { get; set; }

        public virtual string BirthDay { get; set; }

        [Required]
        public virtual string Address1 { get; set; }

        public virtual string Address2 { get; set; }

        [Required]
        public virtual  string City { get; set; }

        [Required]
        public virtual string State { get; set; }

        [Required]
        public virtual string ZipCode { get; set; }

        public virtual string Phone { get; set; }

        public virtual string Mobile { get; set; }
        
        public virtual  string EMailAddr { get; set; }

        [Required]
        public virtual string DateAdded { get; set; }


        // 'Many' side for NH M:M mapping 
        public virtual IList<Asset> Assets { get; set; }

        // NH 'one-side' Investor for Asset relationship.
        public virtual IList<Asset> InvestorAssets { get; set; }


    }
}
