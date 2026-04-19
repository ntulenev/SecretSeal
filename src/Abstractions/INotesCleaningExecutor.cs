namespace Abstractions;

/// <summary>
/// Defines a single cleanup execution for obsolete notes.
/// </summary>
public interface INotesCleaningExecutor
{
    /// <summary>
    /// Executes one cleanup pass.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    Task ExecuteOnceAsync(CancellationToken cancellationToken);
}
