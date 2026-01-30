using Abstractions;

namespace SecretSeal.Services;

/// <summary>
/// Runs the notes cleaner on application startup.
/// </summary>
#pragma warning disable CA1515 // Type used in tests
public sealed class NotesCleanerService : BackgroundService
#pragma warning restore CA1515 
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotesCleanerService"/> class.
    /// </summary>
    /// <param name="cleaningHandler">The handler used to run note cleanup. Cannot be null.</param>
    public NotesCleanerService(INotesCleaningHandler cleaningHandler)
    {
        ArgumentNullException.ThrowIfNull(cleaningHandler);
        _cleaningHandler = cleaningHandler;
    }

    /// <inheritdoc />
    protected async override Task ExecuteAsync(CancellationToken stoppingToken) =>
        await _cleaningHandler.RunAsync(stoppingToken).ConfigureAwait(false);

    private readonly INotesCleaningHandler _cleaningHandler;
}
