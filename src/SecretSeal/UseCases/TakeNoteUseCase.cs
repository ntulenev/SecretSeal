using Abstractions;

using Models;

using Transport;

namespace SecretSeal.UseCases;

/// <summary>
/// Consumes and returns a note for the requested identifier.
/// </summary>
internal sealed class TakeNoteUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TakeNoteUseCase"/> class.
    /// </summary>
    /// <param name="handler">The note handler used to consume notes.</param>
    public TakeNoteUseCase(INotesHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _handler = handler;
    }

    /// <summary>
    /// Consumes and returns the note for the specified identifier.
    /// </summary>
    /// <param name="id">The short identifier of the note to consume.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The consumed note response, or <see langword="null"/> when no note exists for the identifier.</returns>
    public async Task<TakeNoteResponse?> ExecuteAsync(ShortGuid id, CancellationToken cancellationToken)
    {
        var noteId = NoteId.From(id);
        var note = await _handler.TakeNoteAsync(noteId, cancellationToken).ConfigureAwait(false);

        return note is null
            ? null
            : new TakeNoteResponse(note.Id.Value, note.Content);
    }

    private readonly INotesHandler _handler;
}
