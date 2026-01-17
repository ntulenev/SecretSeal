using Abstractions;

using Models;

namespace Logic;

/// <summary>
/// Provides a notes handler backed by a unit of work and repository abstraction.
/// </summary>
public sealed class StorageNotesHandler : INotesHandler
{

    /// <summary>
    /// Initializes a new instance of the StorageNotesHandler class with the specified unit of work.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to access note storage. Cannot be null.</param>
    public StorageNotesHandler(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task AddNoteAsync(Note note, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(note);

        await _unitOfWork.Notes.AddAsync(note, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<long> GetNotesCountAsync(CancellationToken cancellationToken) =>
        _unitOfWork.Notes.CountAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<Note?> TakeNoteAsync(NoteId noteId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(noteId);

        var note = await _unitOfWork.Notes.ConsumeAsync(noteId, cancellationToken).ConfigureAwait(false);

        //Not really needed for current Implementation if ConsumeAsync all executed by RawSQL
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return note is null ? null : note;
    }

    private readonly IUnitOfWork _unitOfWork;
}
