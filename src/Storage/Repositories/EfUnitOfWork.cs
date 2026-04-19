using Abstractions;

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
    /// <param name="notesRepository">The repository that manages note aggregates.</param>
    public EfUnitOfWork(SecretSealDbContext dbContext, INoteRepository notesRepository)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(notesRepository);
        _dbContext = dbContext;
        Notes = notesRepository;
    }

    /// <inheritdoc />
    public INoteRepository Notes { get; }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    private readonly SecretSealDbContext _dbContext;
}
