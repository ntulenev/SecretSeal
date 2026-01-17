namespace Storage.Entities;

internal sealed class DeletedNoteRow
{
    public Guid Id { get; init; }
    public string Content { get; init; } = default!;
}
