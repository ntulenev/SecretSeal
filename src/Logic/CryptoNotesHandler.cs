using Abstractions;

using Models;

namespace Logic;

/// <summary>
/// Provides a secure notes handler that encrypts note content before storage and decrypts it upon retrieval.
/// </summary>
/// <remarks>This class decorates an existing notes handler by transparently applying encryption and
/// decryption to note content. It is useful when note data must be protected at rest, such as when storing
/// sensitive information. All note content added through this handler is encrypted using the provided cryptographic
/// helper, and all content retrieved is decrypted before being returned to the caller.</remarks>
public class CryptoNotesHandler : INotesHandler
{
    /// <summary>
    /// Initializes a new instance of the CryptoNotesHandler class with the specified notes handler and
    /// cryptographic helper.
    /// </summary>
    /// <param name="notesHandler">The notes handler to use for managing note operations. Cannot be null.</param>
    /// <param name="cryptoHelper">The cryptographic helper to use for encryption and decryption operations. 
    /// Cannot be null.</param>
    public CryptoNotesHandler(INotesHandler notesHandler, ICryptoHelper cryptoHelper)
    {
        ArgumentNullException.ThrowIfNull(notesHandler);
        ArgumentNullException.ThrowIfNull(cryptoHelper);
        _notesHandler = notesHandler;
        _cryptoHelper = cryptoHelper;
    }

    /// <summary>
    /// Asynchronously adds a new note to the collection.
    /// </summary>
    /// <remarks>The note's content is encrypted before being stored. This method does not return a
    /// value.</remarks>
    /// <param name="note">The note to add. The note's content must not be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns></returns>
    public async Task AddNoteAsync(Note note, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(note);
        var newNote = new Note(note.Id, _cryptoHelper.Encrypt(note.Content));
        await _notesHandler.AddNoteAsync(newNote, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously retrieves the total number of notes.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the total number of notes.</returns>
    public Task<long> GetNotesCountAsync(CancellationToken cancellationToken) =>
        _notesHandler.GetNotesCountAsync(cancellationToken);

    /// <summary>
    /// Retrieves and decrypts the note associated with the specified note identifier asynchronously.
    /// </summary>
    /// <param name="noteId">The unique identifier of the note to retrieve. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Note"/> object representing the decrypted note if found; 
    /// otherwise, <see langword="null"/>.</returns>
    public async Task<Note?> TakeNoteAsync(NoteId noteId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(noteId);
        var result = await _notesHandler.TakeNoteAsync(noteId, cancellationToken).ConfigureAwait(false);
        return result is null
            ?
            null :
            new Note(result.Id, _cryptoHelper.Decrypt(result.Content));
    }

    private readonly INotesHandler _notesHandler;
    private readonly ICryptoHelper _cryptoHelper;
}
