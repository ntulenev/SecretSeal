using Models;

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

    /// <summary>
    /// Creates a storage entity from a domain <see cref="Note"/>.
    /// </summary>
    /// <param name="note">The note to persist.</param>
    /// <returns>A new <see cref="NoteEntity"/> instance.</returns>
    public static NoteEntity Create(Note note)
    {
        ArgumentNullException.ThrowIfNull(note);

        return new NoteEntity
        {
            Id = note.Id.Value,
            Content = note.Content,
            CreationDate = DateTimeOffset.UtcNow
        };
    }
}
