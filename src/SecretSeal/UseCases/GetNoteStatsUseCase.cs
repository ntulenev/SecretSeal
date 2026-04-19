using Abstractions;

using Logic;

using Microsoft.Extensions.Options;

using SecretSeal.Configuration;

using Transport;

namespace SecretSeal.UseCases;

/// <summary>
/// Returns note storage statistics for the API.
/// </summary>
internal sealed class GetNoteStatsUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetNoteStatsUseCase"/> class.
    /// </summary>
    /// <param name="handler">The note handler used to read note statistics.</param>
    /// <param name="storageOptions">The configured storage options.</param>
    public GetNoteStatsUseCase(INotesHandler handler, IOptions<StorageOptions> storageOptions)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(storageOptions);
        _handler = handler;
        _storageOptions = storageOptions;
    }

    /// <summary>
    /// Returns the current API statistics for note storage.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The current note statistics.</returns>
    public async Task<StatResponse> ExecuteAsync(CancellationToken cancellationToken)
    {
        var count = await _handler.GetNotesCountAsync(cancellationToken).ConfigureAwait(false);
        var encryptionEnabled = _handler is CryptoNotesHandler;
        var isInMemory = _storageOptions.Value.Mode == StorageMode.InMemory;

        return new StatResponse(count, encryptionEnabled, isInMemory);
    }

    private readonly INotesHandler _handler;
    private readonly IOptions<StorageOptions> _storageOptions;
}
