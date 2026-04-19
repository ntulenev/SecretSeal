using Logic.Configuration;

using Microsoft.Extensions.Options;

using SecretSeal.Configuration;

using Transport;

namespace SecretSeal.UseCases;

internal sealed class GetRetentionPolicyUseCase
{
    public GetRetentionPolicyUseCase(
        IOptions<NotesCleanerOptions> notesCleanerOptions,
        IOptions<StorageOptions> storageOptions)
    {
        ArgumentNullException.ThrowIfNull(notesCleanerOptions);
        ArgumentNullException.ThrowIfNull(storageOptions);
        _notesCleanerOptions = notesCleanerOptions;
        _storageOptions = storageOptions;
    }

    public RetentionPolicyResponse Execute()
    {
        var daysToKeep = _storageOptions.Value.Mode == StorageMode.InMemory
            ? -1
            : _notesCleanerOptions.Value.DaysToKeep;

        return new RetentionPolicyResponse(daysToKeep);
    }

    private readonly IOptions<NotesCleanerOptions> _notesCleanerOptions;
    private readonly IOptions<StorageOptions> _storageOptions;
}
