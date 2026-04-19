using Models;

namespace Storage.Entities;

internal sealed class DeletedNoteRow
{
    public Guid Id { get; init; }
    public string Content { get; init; } = default!;

    public Note ToDomainNote() => Note.Restore(Id, Content);
}
