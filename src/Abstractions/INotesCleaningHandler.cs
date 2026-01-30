namespace Abstractions;

/// <summary>
/// Defines a handler for running note cleanup operations.
/// </summary>
public interface INotesCleaningHandler
{
    /// <summary>
    /// Runs the note cleanup operation.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    Task RunAsync(CancellationToken cancellationToken);
}
