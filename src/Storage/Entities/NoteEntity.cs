namespace Storage.Entities;

/// <summary>
/// Represents a note persisted in storage.
/// </summary>
public sealed class NoteEntity
{
    /// <summary>
    /// The unique identifier of the note.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The stored note content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when the note was created.
    /// </summary>
    public DateTimeOffset CreationDate { get; set; }
}
