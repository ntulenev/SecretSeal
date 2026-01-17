using Models;

namespace Abstractions
{
    /// <summary>
    /// Defines methods for adding and retrieving notes asynchronously.
    /// </summary>
    /// <remarks>Implementations of this interface should be thread-safe if used concurrently. Methods support
    /// cancellation via a <see cref="CancellationToken"/> parameter.</remarks>
    public interface INotesHandler
    {
        /// <summary>
        /// Asynchronously adds a new note to the data store.
        /// </summary>
        /// <param name="note">The note to add. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous add operation.</returns>
        Task AddNoteAsync(Note note, CancellationToken cancellationToken);

        /// <summary>
        /// Attempts to asynchronously retrieve a note by its identifier.
        /// </summary>
        /// <param name="noteId">The unique identifier of the note to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <param name="note">When this method returns, contains the note associated with the specified identifier, if found; otherwise,
        /// the default value for <see cref="Note"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the note was
        /// found and retrieved successfully; otherwise, <see langword="false"/>.</returns>
        Task<bool> TryReadNoteAsync(NoteId noteId, CancellationToken cancellationToken, out Note note);

        /// <summary>
        /// Asynchronously retrieves the total number of notes available.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total number of notes as a
        /// 64-bit integer.</returns>
        Task<long> GetNotesCountAsync(CancellationToken cancellationToken);
    }
}
