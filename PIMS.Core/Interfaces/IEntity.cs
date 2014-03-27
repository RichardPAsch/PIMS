using System;


namespace PIMS.Core.Interfaces
{
    // Ensures any derived entity will also include a Guid key, e.g. for Linq lookups.
    public interface IEntity
    {
        Guid KeyId { get; set; }
    }
}
