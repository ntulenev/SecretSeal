using FluentAssertions;

namespace Models.Tests;

public sealed class NoteTests
{
    [Fact(DisplayName = "Constructor throws when id is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenIdIsNullThrowsArgumentNullException()
    {
        //Arrage
        NoteId id = null!;
        var content = "content";

        //Act
        Action act = () => _ = new Note(id, content);

        //Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when content is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenContentIsNullThrowsArgumentNullException()
    {
        //Arrage
        var id = new NoteId(Guid.NewGuid());
        string content = null!;

        //Act
        Action act = () => _ = new Note(id, content);

        //Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when content is empty or whitespace")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenContentIsWhitespaceThrowsArgumentException()
    {
        //Arrage
        var id = new NoteId(Guid.NewGuid());
        var content = "   ";

        //Act
        Action act = () => _ = new Note(id, content);

        //Assert
        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Constructor throws when content is empty string")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenContentIsEmptyThrowsArgumentException()
    {
        //Arrage
        var id = new NoteId(Guid.NewGuid());
        var content = string.Empty;

        //Act
        Action act = () => _ = new Note(id, content);

        //Assert
        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Constructor sets id and content")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenArgumentsAreValidSetsProperties()
    {
        //Arrage
        var id = new NoteId(Guid.NewGuid());
        var content = "hello";

        //Act
        var note = new Note(id, content);

        //Assert
        note.Id.Should().Be(id);
        note.Content.Should().Be(content);
    }

    [Fact(DisplayName = "Create makes a note with a new id and content")]
    [Trait("Category", "Unit")]
    public void CreateWhenContentIsValidCreatesNote()
    {
        //Arrage
        var content = "hello";

        //Act
        var note = Note.Create(content);

        //Assert
        note.Id.Should().NotBeNull();
        note.Id.Value.Should().NotBe(Guid.Empty);
        note.Content.Should().Be(content);
    }
}
