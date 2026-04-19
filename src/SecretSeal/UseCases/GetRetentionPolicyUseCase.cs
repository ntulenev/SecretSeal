using Logic.Configuration;

using Microsoft.Extensions.Options;

using SecretSeal.Configuration;

using Transport;

namespace SecretSeal.UseCases;

/// <summary>
/// Returns the effective note retention policy for the API.
/// </summary>
internal sealed class GetRetentionPolicyUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetRetentionPolicyUseCase"/> class.
    /// </summary>
    /// <param name="notesCleanerOptions">The configured note cleanup options.</param>
    /// <param name="storageOptions">The configured storage options.</param>
    public GetRetentionPolicyUseCase(
        IOptions<NotesCleanerOptions> notesCleanerOptions,
        IOptions<StorageOptions> storageOptions)
    {
        ArgumentNullException.ThrowIfNull(notesCleanerOptions);
        ArgumentNullException.ThrowIfNull(storageOptions);
        _notesCleanerOptions = notesCleanerOptions;
        _storageOptions = storageOptions;
    }

    /// <summary>
    /// Returns the effective retention policy visible to API consumers.
    /// </summary>
    /// <returns>The current retention policy response.</returns>
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
