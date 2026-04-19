using Models;

namespace Storage.Entities;

/// <summary>
/// Represents the raw row returned by the delete-output query for notes.
/// </summary>
internal sealed class DeletedNoteRow
{
    /// <summary>
    /// Gets the identifier of the deleted note.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the deleted note content.
    /// </summary>
    public string Content { get; init; } = default!;

    /// <summary>
    /// Converts the deleted-row data into the domain note model.
    /// </summary>
    /// <returns>A <see cref="Note"/> restored from the deleted row values.</returns>
    public Note ToDomainNote() => Note.Restore(Id, Content);
}
