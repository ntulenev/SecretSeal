using Microsoft.EntityFrameworkCore;

using Storage.Entities;

namespace Storage;

/// <summary>
/// EF Core database context for SecretSeal storage.
/// </summary>
public sealed class SecretSealDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the SecretSealDbContext class.
    /// </summary>
    /// <param name="options">The options to configure the context.</param>
    public SecretSealDbContext(DbContextOptions<SecretSealDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Notes persisted in storage.
    /// </summary>
    public DbSet<NoteEntity> Notes => Set<NoteEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        _ = modelBuilder.Entity<DeletedNoteRow>().HasNoKey();
        _ = modelBuilder.Entity<NoteEntity>(entity =>
        {
            _ = entity.ToTable("Notes");
            _ = entity.HasKey(note => note.Id);
            _ = entity.Property(note => note.Content)
                .IsRequired();
            _ = entity.Property(note => note.CreationDate)
                .IsRequired();
        });
    }
}
