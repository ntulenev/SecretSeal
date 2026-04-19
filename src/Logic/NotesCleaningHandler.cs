using Abstractions;

using Logic.Configuration;

using Microsoft.Extensions.Options;

namespace Logic;

/// <summary>
/// Runs note cleanup on a periodic loop.
/// </summary>
public sealed class NotesCleaningHandler : INotesCleaningHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotesCleaningHandler"/> class.
    /// </summary>
    /// <param name="executor">The executor that performs one cleanup pass. Cannot be null.</param>
    /// <param name="options">The options defining how often cleanup runs. Cannot be null.</param>
    public NotesCleaningHandler(INotesCleaningExecutor executor, IOptions<NotesCleanerOptions> options)
    {
        ArgumentNullException.ThrowIfNull(executor);
        ArgumentNullException.ThrowIfNull(options);

        if (options.Value is null)
        {
            throw new ArgumentException("Notes cleaner options are not configured.", nameof(options));
        }

        _executor = executor;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _executor.ExecuteOnceAsync(cancellationToken).ConfigureAwait(false);
            await Task.Delay(_options.CleanupInterval, cancellationToken).ConfigureAwait(false);
        }
    }

    private readonly INotesCleaningExecutor _executor;
    private readonly NotesCleanerOptions _options;
}
