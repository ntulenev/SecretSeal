using Abstractions;

using Microsoft.EntityFrameworkCore;

using Models;

using Storage.Entities;

namespace Storage.Repositories;

/// <summary>
/// Provides repository operations for notes using EF Core.
/// </summary>
public sealed class NotesRepository : IRepository<Note, NoteId>
{
    /// <summary>
    /// Initializes a new instance of the NotesRepository class.
    /// </summary>
    /// <param name="dbContext">The database context used for storage operations.</param>
    public NotesRepository(SecretSealDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public Task AddAsync(Note entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var storageEntity = new NoteEntity
        {
            Id = entity.Id.Value,
            Content = entity.Content,
            CreationDate = DateTimeOffset.UtcNow
        };

        _ = _dbContext.Notes.Add(storageEntity);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes the note with the specified identifier and returns the deleted note if it existed.
    /// </summary>
    /// <param name="id">The identifier of the note to delete. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Note"/> representing the deleted note if the note was found and deleted; otherwise, <see
    /// langword="null"/>.</returns>
    public async Task<Note?> ConsumeAsync(NoteId id, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(id);

        var row = await _dbContext.Set<DeletedNoteRow>()
            .FromSqlRaw("""
            DELETE FROM Notes
            OUTPUT DELETED.Id, DELETED.Content
            WHERE Id = {0};
        """, id.Value)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return row is null
            ? null
            : new Note(new NoteId(row.Id), row.Content);
    }

    /// <inheritdoc />
    public Task<long> CountAsync(CancellationToken cancellationToken) =>
        _dbContext.Notes.LongCountAsync(cancellationToken);

    private readonly SecretSealDbContext _dbContext;
}
