using Abstractions;

using Models;

namespace Storage.Repositories;

/// <summary>
/// Provides EF Core unit of work implementation.
/// </summary>
public sealed class EfUnitOfWork : IUnitOfWork
{
    /// <summary>
    /// Initializes a new instance of the EfUnitOfWork class.
    /// </summary>
    /// <param name="dbContext">The database context used for persistence.</param>
    public EfUnitOfWork(SecretSealDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
        Notes = new NotesRepository(_dbContext);
    }

    /// <inheritdoc />
    public IRepository<Note, NoteId> Notes { get; }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    private readonly SecretSealDbContext _dbContext;
}
