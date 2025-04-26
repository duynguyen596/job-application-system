using System.Linq.Expressions;

namespace JobApplicationSystem.Domain.Cores;

/// <summary>
/// Generic base interface for repository operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TId">The type of the entity's primary key.</typeparam>
public interface IBaseRepository<T, TId> where T : IEntity<TId>
{
    /// <summary>
    /// Gets an entity by its unique identifier.
    /// Implementations should decide on loading related data strategy (e.g., default, none).
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity or null if not found.</returns>
    Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities of this type.
    /// Implementations should be cautious about performance on large tables. Consider adding filtering/paging specific methods.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all entities.</returns>
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities based on a predicate expression.
    /// </summary>
    /// <param name="predicate">The condition to filter entities.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of matching entities.</returns>
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

     /// <summary>
    /// Gets the first entity matching the predicate or null if not found.
    /// </summary>
    /// <param name="predicate">The condition to filter entities.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first matching entity or null.</returns>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the data store.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing entity for update. (Actual DB update occurs on SaveChangesAsync).
    /// </summary>
    /// <param name="entity">The entity with updated values.</param>
    void Update(T entity);

    /// <summary>
    /// Marks an existing entity for deletion. (Actual DB deletion occurs on SaveChangesAsync).
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    void Delete(T entity);

    /// <summary>
    /// Checks if an entity with the given ID exists.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the entity exists, otherwise false.</returns>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);

    // Consider adding CountAsync, AddRangeAsync, DeleteRange if commonly needed
    // Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    // Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    // void DeleteRange(IEnumerable<T> entities);
}