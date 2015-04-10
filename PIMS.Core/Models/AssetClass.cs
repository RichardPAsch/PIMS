using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PIMS.Core.Interfaces;


namespace PIMS.Core.Models
{
    /* Property names MUST match table column names, IF not noted otherwise in mapping classes. */

    public class AssetClass : IEntity
    {
        public virtual string Url { get; set; }
        
        [Key]
        public virtual Guid KeyId { get; set; }  // Mapping: AssetClassId

        // Example: "CS"
        [Required]
        public virtual string Code { get; set; }


        // Example: "Common Stock"
        [Required]
        public virtual string Description { get; set; }


        // NH relational mapping for 'many' side of M:1 Asset/AssetClassification.
        public virtual IList<Asset> AssetClassificationAssets { get; set; }
    

    }
}
