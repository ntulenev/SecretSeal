using Abstractions;
using Models;
using System.Collections.Concurrent;

namespace Logic;

/// <summary>
/// Provides an in-memory implementation of the INotesHandler interface for managing notes during application runtime.
/// </summary>
/// <remarks>This handler stores notes in memory only and does not persist data between application restarts. It
/// is suitable for testing, prototyping, or scenarios where persistent storage is not required. This class is not
/// thread-safe for concurrent access unless otherwise specified by the consuming code.</remarks>
public class InMemoryNotesHandler : INotesHandler
{
    /// <summary>
    /// Asynchronously adds the specified note to the collection, replacing any existing note with the same identifier.
    /// </summary>
    /// <param name="note">The note to add to the collection. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous add operation.</returns>
    public Task AddNoteAsync(Note note, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(note);

        _notes[note.Id] = note;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Attempts to read and remove a note with the specified identifier asynchronously.
    /// </summary>
    /// <remarks>If the note is found, it is removed from the underlying collection. Subsequent calls with the
    /// same identifier will not retrieve the same note.</remarks>
    /// <param name="noteId">The identifier of the note to read and remove. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <param name="note">When this method returns, contains the note associated with the specified identifier, 
    /// if found; otherwise, the
    /// default value for <see cref="Note"/>. This parameter is passed uninitialized.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the note was
    /// found and removed; otherwise, <see langword="false"/>.</returns>
    public Task<bool> TryReadNoteAsync(NoteId noteId, CancellationToken cancellationToken, out Note note)
    {
        ArgumentNullException.ThrowIfNull(noteId);

        if (!_notes.TryRemove(noteId, out var encrypted))
        {
            note = default!;
            return Task.FromResult(false);
        }

        note = encrypted;
        return Task.FromResult(true);
    }

    /// <summary>
    /// Asynchronously gets the total number of notes currently stored.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the total number of notes.</returns>
    public Task<long> GetNotesCountAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult((long)_notes.Count);
    }

    private readonly ConcurrentDictionary<NoteId, Note> _notes = new();
}
