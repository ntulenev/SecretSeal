using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Models;

using Storage.Entities;
using Storage.Repositories;

namespace Storage.Tests;

public sealed class NotesRepositorySqlServerTests
{
    [Fact(DisplayName = "AddAsync persists note entity values on SQL Server")]
    [Trait("Category", "Integration")]
    public async Task AddAsyncPersistsNoteEntityValues()
    {
        // Arrange
        await using var database = await SqlServerTestDatabase.CreateAsync();
        var repository = new NotesRepository(database.Context);
        var note = Note.Create("stored");
        var before = DateTimeOffset.UtcNow;

        // Act
        await repository.AddAsync(note, CancellationToken.None);
        await database.Context.SaveChangesAsync();

        await using var verificationContext = database.CreateContext();
        var stored = await verificationContext.Notes.SingleAsync();
        var after = DateTimeOffset.UtcNow;

        // Assert
        stored.Id.Should().Be(note.Id.Value);
        stored.Content.Should().Be(note.Content);
        stored.CreationDate.Should().BeOnOrAfter(before);
        stored.CreationDate.Should().BeOnOrBefore(after);
    }

    [Fact(DisplayName = "ConsumeAsync deletes note and returns deleted domain note on SQL Server")]
    [Trait("Category", "Integration")]
    public async Task ConsumeAsyncDeletesNoteAndReturnsDeletedNote()
    {
        // Arrange
        await using var database = await SqlServerTestDatabase.CreateAsync();
        var note = Note.Create("delete me");
        var repository = new NotesRepository(database.Context);

        await repository.AddAsync(note, CancellationToken.None);
        await database.Context.SaveChangesAsync();

        // Act
        var deleted = await repository.ConsumeAsync(note.Id, CancellationToken.None);

        // Assert
        deleted.Should().NotBeNull();
        deleted.Should().Be(note);

        await using var verificationContext = database.CreateContext();
        var remainingCount = await verificationContext.Notes.LongCountAsync();
        remainingCount.Should().Be(0);
    }

    [Fact(DisplayName = "ConsumeAsync returns null when note does not exist on SQL Server")]
    [Trait("Category", "Integration")]
    public async Task ConsumeAsyncReturnsNullWhenNoteDoesNotExist()
    {
        // Arrange
        await using var database = await SqlServerTestDatabase.CreateAsync();
        var repository = new NotesRepository(database.Context);

        // Act
        var deleted = await repository.ConsumeAsync(NoteId.New(), CancellationToken.None);

        // Assert
        deleted.Should().BeNull();
    }

    [Fact(DisplayName = "RemoveObsoleteNotesAsync deletes only notes older than cutoff on SQL Server")]
    [Trait("Category", "Integration")]
    public async Task RemoveObsoleteNotesAsyncDeletesOnlyObsoleteNotes()
    {
        // Arrange
        await using var database = await SqlServerTestDatabase.CreateAsync();
        var oldId = Guid.NewGuid();
        var freshId = Guid.NewGuid();
        var cutoff = DateTimeOffset.UtcNow.AddDays(-7);

        database.Context.Notes.AddRange(
            new NoteEntity
            {
                Id = oldId,
                Content = "old",
                CreationDate = cutoff.AddMinutes(-1)
            },
            new NoteEntity
            {
                Id = freshId,
                Content = "fresh",
                CreationDate = cutoff.AddMinutes(1)
            });
        await database.Context.SaveChangesAsync();

        var repository = new NotesRepository(database.Context);

        // Act
        var removedCount = await repository.RemoveObsoleteNotesAsync(cutoff, CancellationToken.None);

        // Assert
        removedCount.Should().Be(1);

        await using var verificationContext = database.CreateContext();
        var remainingNotes = await verificationContext.Notes
            .OrderBy(note => note.Content)
            .ToListAsync();

        remainingNotes.Should().HaveCount(1);
        remainingNotes[0].Id.Should().Be(freshId);
        remainingNotes[0].Content.Should().Be("fresh");
    }
}
