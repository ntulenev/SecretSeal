using Abstractions;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Storage.Entities;
using Storage.Repositories;

namespace Storage.Tests;

public sealed class EfUnitOfWorkTests
{
    [Fact(DisplayName = "Constructor throws when db context is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenDbContextIsNullThrowsArgumentNullException()
    {
        // Arrange
        SecretSealDbContext dbContext = null!;
        var notesRepository = new StubNoteRepository();

        // Act
        Action act = () => _ = new EfUnitOfWork(dbContext, notesRepository);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when notes repository is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenNotesRepositoryIsNullThrowsArgumentNullException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        INoteRepository notesRepository = null!;

        // Act
        Action act = () => _ = new EfUnitOfWork(context, notesRepository);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Notes returns repository passed to constructor")]
    [Trait("Category", "Unit")]
    public void NotesReturnsRepositoryPassedToConstructor()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var notesRepository = new StubNoteRepository();

        // Act
        var unitOfWork = new EfUnitOfWork(context, notesRepository);

        // Assert
        unitOfWork.Notes.Should().BeSameAs(notesRepository);
    }

    [Fact(DisplayName = "SaveChangesAsync persists tracked entities")]
    [Trait("Category", "Unit")]
    public async Task SaveChangesAsyncPersistsTrackedEntities()
    {
        // Arrange
        await using var database = await SqlServerTestDatabase.CreateAsync();
        var repository = new NotesRepository(database.Context);
        var unitOfWork = new EfUnitOfWork(database.Context, repository);

        database.Context.Notes.Add(new NoteEntity
        {
            Id = Guid.NewGuid(),
            Content = "persisted",
            CreationDate = DateTimeOffset.UtcNow
        });

        // Act
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        // Assert
        await using var verificationContext = database.CreateContext();
        var count = await verificationContext.Notes.LongCountAsync();
        count.Should().Be(1);
    }

    private static SecretSealDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<SecretSealDbContext>()
            .UseInMemoryDatabase($"EfUnitOfWork-{Guid.NewGuid()}")
            .Options;

        return new SecretSealDbContext(options);
    }

    private sealed class StubNoteRepository : INoteRepository
    {
        public Task AddAsync(Models.Note note, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<Models.Note?> ConsumeAsync(Models.NoteId id, CancellationToken cancellationToken) =>
            Task.FromResult<Models.Note?>(null);

        public Task<long> CountAsync(CancellationToken cancellationToken) => Task.FromResult(0L);

        public Task<int> RemoveObsoleteNotesAsync(DateTimeOffset olderThan, CancellationToken cancellationToken) =>
            Task.FromResult(0);
    }
}
