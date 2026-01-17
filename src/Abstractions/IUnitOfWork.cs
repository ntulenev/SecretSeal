using Models;

namespace Abstractions;

/// <summary>
/// Defines a unit of work for coordinating note storage operations.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Gets the notes repository.
    /// </summary>
    IRepository<Note, NoteId> Notes { get; }

    /// <summary>
    /// Persists pending changes to the underlying store.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
