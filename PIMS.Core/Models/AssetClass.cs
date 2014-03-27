using System;
using System.ComponentModel.DataAnnotations;
using PIMS.Core.Interfaces;


namespace PIMS.Core.Models
{
    /* Property names MUST match table columns. */

    public class AssetClass : IEntity
    {
        [Key]
        public virtual Guid KeyId { get; set; }

        // Example: "CS"
        [Required]
        public virtual string Code { get; set; }


        // Example: "Common Stock"
        [Required]
        public virtual string Description { get; set; }

    }
}
