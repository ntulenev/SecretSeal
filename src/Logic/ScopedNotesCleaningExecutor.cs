using Abstractions;

using Microsoft.Extensions.DependencyInjection;

namespace Logic;

/// <summary>
/// Resolves and executes the notes cleaner inside a dedicated scope.
/// </summary>
public sealed class ScopedNotesCleaningExecutor : INotesCleaningExecutor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScopedNotesCleaningExecutor"/> class.
    /// </summary>
    /// <param name="scopeFactory">The scope factory used to resolve scoped services. Cannot be null.</param>
    public ScopedNotesCleaningExecutor(IServiceScopeFactory scopeFactory)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc />
    public async Task ExecuteOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var cleaner = scope.ServiceProvider.GetRequiredService<INotesCleaner>();
        await cleaner.RemoveObsoleteNotesAsync(cancellationToken).ConfigureAwait(false);
    }

    private readonly IServiceScopeFactory _scopeFactory;
}
