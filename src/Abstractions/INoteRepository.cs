using Models;

namespace Abstractions;

/// <summary>
/// Defines repository operations for note aggregates.
/// </summary>
public interface INoteRepository
{
    /// <summary>
    /// Adds or replaces the specified note in the repository.
    /// </summary>
    /// <param name="note">The note to add. Cannot be null.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    Task AddAsync(Note note, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves and consumes the note with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the note to retrieve.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The note if found; otherwise, null.</returns>
    Task<Note?> ConsumeAsync(NoteId id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the total number of notes in the repository.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The number of notes.</returns>
    Task<long> CountAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Removes all notes older than the specified timestamp.
    /// </summary>
    /// <param name="olderThan">The cutoff timestamp; notes older than this value are removed.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The number of notes removed.</returns>
    Task<int> RemoveObsoleteNotesAsync(DateTimeOffset olderThan, CancellationToken cancellationToken);
}
