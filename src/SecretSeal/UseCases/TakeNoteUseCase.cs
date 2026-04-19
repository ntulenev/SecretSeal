using Abstractions;

using Models;

using Transport;

namespace SecretSeal.UseCases;

internal sealed class TakeNoteUseCase
{
    public TakeNoteUseCase(INotesHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _handler = handler;
    }

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
