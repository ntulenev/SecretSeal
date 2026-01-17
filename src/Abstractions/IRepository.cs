namespace Abstractions;

/// <summary>
/// Defines basic repository operations for aggregates identified by a key.
/// </summary>
/// <typeparam name="TEntity">The entity type managed by the repository.</typeparam>
/// <typeparam name="TKey">The type of the entity identifier.</typeparam>
public interface IRepository<TEntity, TKey>
{
    /// <summary>
    /// Adds or replaces the specified entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to add. Cannot be null.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the entity with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<TEntity?> ConsumeAsync(TKey id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the total number of entities in the repository.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The number of entities.</returns>
    Task<long> CountAsync(CancellationToken cancellationToken);
}
