namespace Transport;

/// <summary>
/// Represents the result of a statistics query containing the total number of notes.
/// </summary>
/// <param name="NotesCount">The total number of notes returned by the statistics query.</param>
/// <param name="EncryptionEnabled">Indicates whether encryption is enabled for note storage.</param>
public sealed record StatResponse(long NotesCount, bool EncryptionEnabled);