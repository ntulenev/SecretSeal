using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Models;

using Storage.Entities;

namespace Storage.Tests;

public sealed class StorageEntitiesTests
{
    [Fact(DisplayName = "NoteEntity.Create throws when note is null")]
    [Trait("Category", "Unit")]
    public void NoteEntityCreateWhenNoteIsNullThrowsArgumentNullException()
    {
        // Arrange
        Note note = null!;

        // Act
        Action act = () => _ = NoteEntity.Create(note);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "NoteEntity.Create maps note values and timestamp")]
    [Trait("Category", "Unit")]
    public void NoteEntityCreateMapsValuesAndTimestamp()
    {
        // Arrange
        var note = Note.Create("mapped");
        var before = DateTimeOffset.UtcNow;

        // Act
        var entity = NoteEntity.Create(note);
        var after = DateTimeOffset.UtcNow;

        // Assert
        entity.Id.Should().Be(note.Id.Value);
        entity.Content.Should().Be(note.Content);
        entity.CreationDate.Should().BeOnOrAfter(before);
        entity.CreationDate.Should().BeOnOrBefore(after);
    }

    [Fact(DisplayName = "DeletedNoteRow converts to domain note")]
    [Trait("Category", "Unit")]
    public void DeletedNoteRowToDomainNoteReturnsRestoredNote()
    {
        // Arrange
        var id = Guid.NewGuid();
        var row = new DeletedNoteRow
        {
            Id = id,
            Content = "restored"
        };

        // Act
        var note = row.ToDomainNote();

        // Assert
        note.Id.Value.Should().Be(id);
        note.Content.Should().Be("restored");
    }

    [Fact(DisplayName = "DbContext exposes notes set and configures note entity model")]
    [Trait("Category", "Unit")]
    public void DbContextConfiguresNoteEntityModel()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        var noteEntityType = context.Model.FindEntityType(typeof(NoteEntity));
        var deletedRowType = context.Model.FindEntityType(typeof(DeletedNoteRow));

        // Assert
        context.Notes.Should().NotBeNull();
        noteEntityType.Should().NotBeNull();
        noteEntityType!.FindPrimaryKey()!.Properties.Select(property => property.Name).Should().ContainSingle().Which.Should().Be(nameof(NoteEntity.Id));
        noteEntityType.FindProperty(nameof(NoteEntity.Content))!.IsNullable.Should().BeFalse();
        noteEntityType.FindProperty(nameof(NoteEntity.CreationDate))!.IsNullable.Should().BeFalse();
        noteEntityType.GetIndexes().Select(index => index.Properties.Single().Name).Should().Contain(nameof(NoteEntity.CreationDate));

        deletedRowType.Should().NotBeNull();
        deletedRowType!.FindPrimaryKey().Should().BeNull();
    }

    private static SecretSealDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<SecretSealDbContext>()
            .UseInMemoryDatabase($"StorageEntities-{Guid.NewGuid()}")
            .Options;

        return new SecretSealDbContext(options);
    }
}
