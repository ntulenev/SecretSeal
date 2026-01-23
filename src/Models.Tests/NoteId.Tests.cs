using FluentAssertions;

namespace Models.Tests;

public sealed class NoteIdTests
{
    [Fact(DisplayName = "Constructor throws when value is empty")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValueIsEmptyThrowsArgumentException()
    {
        // Arrange
        var value = Guid.Empty;

        // Act
        Action act = () => _ = new NoteId(value);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Constructor sets value when value is not empty")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValueIsNotEmptySetsValue()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var noteId = new NoteId(value);

        // Assert
        noteId.Value.Should().Be(value);
    }

    [Fact(DisplayName = "New creates a NoteId with a non-empty value")]
    [Trait("Category", "Unit")]
    public void NewCreatesNoteIdWithNonEmptyValue()
    {
        // Arrange & Act
        var noteId = NoteId.New();

        // Assert
        noteId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "Equals and GetHashCode use value semantics")]
    [Trait("Category", "Unit")]
    public void EqualsAndGetHashCodeUseValueSemantics()
    {
        // Arrange
        var value = Guid.NewGuid();
        var first = new NoteId(value);
        var second = new NoteId(value);

        // Act
        var areEqual = first.Equals(second);
        var firstHash = first.GetHashCode();
        var secondHash = second.GetHashCode();

        // Assert
        areEqual.Should().BeTrue();
        firstHash.Should().Be(secondHash);
    }
}
