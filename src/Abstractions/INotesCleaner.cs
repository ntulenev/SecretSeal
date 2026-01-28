namespace Abstractions;

/// <summary>
/// Defines methods for removing obsolete notes asynchronously.
/// </summary>
/// <remarks>Implementations of this interface should be thread-safe if used concurrently. Methods support
/// cancellation via a <see cref="CancellationToken"/> parameter.</remarks>
public interface INotesCleaner
{
    /// <summary>
    /// Asynchronously removes obsolete notes.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous cleanup operation.</returns>
    Task RemoveObsoleteNotesAsync(CancellationToken cancellationToken);
}
