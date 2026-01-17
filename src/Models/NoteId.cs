namespace Models;

/// <summary>
/// Represents a unique identifier for a note.
/// </summary>
public sealed record NoteId
{
    /// <summary>
    /// The unique identifier value.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the NoteId struct with the specified unique identifier.
    /// </summary>
    /// <param name="value">The unique identifier value for the note. Must not be <see cref="Guid.Empty"/>.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    public NoteId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("NoteId value cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="NoteId"/> with a randomly generated GUID.
    /// </summary>
    /// <returns>A new <see cref="NoteId"/> instance.</returns>
    public static NoteId New() => new(Guid.NewGuid());
}
