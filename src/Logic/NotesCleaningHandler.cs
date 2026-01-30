using Abstractions;

using Logic.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Logic;

/// <summary>
/// Resolves and executes the notes cleaner in an isolated scope.
/// </summary>
public sealed class NotesCleaningHandler : INotesCleaningHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotesCleaningHandler"/> class.
    /// </summary>
    /// <param name="scopeFactory">The scope factory used to resolve scoped services. Cannot be null.</param>
    /// <param name="options">The options defining how often cleanup runs. Cannot be null.</param>
    public NotesCleaningHandler(IServiceScopeFactory scopeFactory, IOptions<NotesCleanerOptions> options)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(options);

        if (options.Value is null)
        {
            throw new ArgumentException("Notes cleaner options are not configured.", nameof(options));
        }

        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var scope = _scopeFactory.CreateScope();
            var cleaner = scope.ServiceProvider.GetRequiredService<INotesCleaner>();
            await cleaner.RemoveObsoleteNotesAsync(cancellationToken).ConfigureAwait(false);

            await Task.Delay(_options.CleanupInterval, cancellationToken).ConfigureAwait(false);
        }
    }

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly NotesCleanerOptions _options;
}
