namespace Models;

/// <summary>
/// Represents a Secret Note.
/// </summary>
public sealed record Note
{
    /// <summary>
    /// The unique identifier of the note.
    /// </summary>
    public NoteId Id { get; }

    /// <summary>
    /// The content of the note.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Initializes a new instance of the Note class with the specified identifier, content, and creation date.
    /// </summary>
    /// <param name="id">The unique identifier for the note. Cannot be null.</param>
    /// <param name="content">The text content of the note. Cannot be null, empty, or consist only of whitespace.</param>
    /// <exception cref="ArgumentException">Thrown if content is empty or consists only of whitespace.</exception>
    public Note(NoteId id, string content)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(content);

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Note content cannot be empty or whitespace.", nameof(content));
        }

        Id = id;
        Content = content;
    }

    /// <summary>
    /// Creates a copy of this <see cref="Note"/> with updated content.
    /// </summary>
    /// <param name="content">The new note content.</param>
    /// <returns>A new <see cref="Note"/> instance that keeps the same identifier.</returns>
    public Note WithContent(string content) => new(Id, content);

    /// <summary>
    /// Rehydrates a <see cref="Note"/> from persisted values.
    /// </summary>
    /// <param name="id">The persisted note identifier.</param>
    /// <param name="content">The persisted note content.</param>
    /// <returns>A rehydrated <see cref="Note"/> instance.</returns>
    public static Note Restore(Guid id, string content) => new(NoteId.From(id), content);

    /// <summary>
    /// Creates a new <see cref="Note"/> with a newly generated identifier and the current time.
    /// </summary>
    /// <param name="content">The content of the note.</param>
    /// <returns>A new <see cref="Note"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="content"/> is empty or whitespace.</exception>
    public static Note Create(string content) => new(NoteId.New(), content);
}
