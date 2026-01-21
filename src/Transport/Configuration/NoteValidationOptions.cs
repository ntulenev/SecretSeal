namespace Transport.Configuration;

/// <summary>
/// Defines validation-related configuration for note input.
/// </summary>
public sealed class NoteValidationOptions
{
    /// <summary>
    /// Optional maximum allowed length of a note (in characters).
    /// If null, length is not limited by configuration.
    /// </summary>
    public int? MaxNoteLength { get; init; }
}
