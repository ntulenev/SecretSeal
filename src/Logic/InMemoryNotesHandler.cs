using Abstractions;
using Models;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

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
    /// Retrieves and removes the note associated with the specified identifier, if it exists.
    /// </summary>
    /// <param name="noteId">The unique identifier of the note to retrieve and remove. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the note if found; 
    /// otherwise, null.</returns>
    public Task<Note?> TakeNoteAsync(NoteId noteId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(noteId);

        if (!_notes.TryRemove(noteId, out var note))
        {
            return Task.FromResult<Note?>(null!);
        }
        return Task.FromResult<Note?>(note);
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
