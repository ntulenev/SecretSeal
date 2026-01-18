namespace SecretSeal.Configuration;

/// <summary>
/// Defines storage-related configuration settings for the SecretSeal application.
/// </summary>
internal sealed class StorageOptions
{
    /// <summary>
    /// Specifies which storage backend should be used for persisting notes.
    /// </summary>
    public required StorageMode Mode { get; init; }

    /// <summary>
    /// Optional maximum allowed length of a note (in characters).
    /// If null, length is not limited by configuration.
    /// </summary>
    public int? MaxNoteLength { get; init; }
}
