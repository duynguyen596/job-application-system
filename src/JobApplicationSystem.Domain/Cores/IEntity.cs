namespace JobApplicationSystem.Domain.Cores; // Or JobApplicationSystem.Domain.Primitives

/// <summary>
/// Defines a generic entity interface that exposes an Id property.
/// This allows generic repositories and services to work with entity identifiers
/// in a type-safe way.
/// </summary>
/// <typeparam name="TId">The data type of the entity's unique identifier.</typeparam>
public interface IEntity<TId>
{
    /// <summary>
    /// Gets or sets the unique identifier for this entity.
    /// </summary>
    TId Id { get; set; }
}