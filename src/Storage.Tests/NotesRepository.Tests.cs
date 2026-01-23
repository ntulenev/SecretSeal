using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Models;

using Storage.Repositories;

namespace Storage.Tests;

public sealed class NotesRepositoryTests
{
    //!!! ConsumeAsync Tests limited due to InMemory provider limitations with FromSqlRaw

    [Fact(DisplayName = "AddAsync throws when note is null")]
    [Trait("Category", "Unit")]
    public async Task AddAsyncWhenNoteIsNullThrowsArgumentNullException()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new NotesRepository(context);
        Note note = null!;
        var cancellationToken = new CancellationToken();

        // Act
        Func<Task> act = () => repository.AddAsync(note, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact(DisplayName = "RemoveAsync throws when note id is null")]
    [Trait("Category", "Unit")]
    public async Task RemoveAsyncWhenNoteIdIsNullThrowsArgumentNullException()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new NotesRepository(context);
        NoteId noteId = null!;
        var cancellationToken = new CancellationToken();

        // Act
        Func<Task> act = () => repository.ConsumeAsync(noteId, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact(DisplayName = "CountAsync returns number of notes")]
    [Trait("Category", "Unit")]
    public async Task CountAsyncReturnsNumberOfNotes()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new NotesRepository(context);
        var cancellationToken = new CancellationToken();

        await repository.AddAsync(new Note(new NoteId(Guid.NewGuid()), "one"), cancellationToken);
        await repository.AddAsync(new Note(new NoteId(Guid.NewGuid()), "two"), cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // Act
        var count = await repository.CountAsync(cancellationToken);

        // Assert
        count.Should().Be(2);
        count.Should().Be(2);
    }

    private static SecretSealDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SecretSealDbContext>()
            .UseInMemoryDatabase($"SecretSeal-{Guid.NewGuid()}")
            .Options;

        return new SecretSealDbContext(options);
    }
}
