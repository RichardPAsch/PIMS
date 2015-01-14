using System;


namespace PIMS.Data.Repositories
{
    public interface IGenericAggregateRepository
    {
        // Handles working with Aggregate Roots (AssetRepository) where CRUD issues surrounding child aggregates, e.g., Position, Income, or Profile,
        // and their repositories, now have their responsibilities delegated to the parent root aggregate, avoiding circular references & complicated
        // references among child repositories.

        bool AggregateCreate<T>(T newEntity, string investor, string ticker);
        bool AggregateDelete<T>(Guid keyId);
        bool AggregateUpdate<T>(T editedEntity, string investor, string ticker);
    }
}
