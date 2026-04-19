using Abstractions;

using Logic;

using Microsoft.Extensions.Options;

using SecretSeal.Configuration;

using Transport;

namespace SecretSeal.UseCases;

internal sealed class GetNoteStatsUseCase
{
    public GetNoteStatsUseCase(INotesHandler handler, IOptions<StorageOptions> storageOptions)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(storageOptions);
        _handler = handler;
        _storageOptions = storageOptions;
    }

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
