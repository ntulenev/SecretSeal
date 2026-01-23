using FluentAssertions;

using Microsoft.Extensions.Options;

using Transport.Configuration;
using Transport.Validation;

namespace Transport.Tests;

public sealed class NoteValidatorTests
{
    [Fact(DisplayName = "Ctor throws when options is null")]
    [Trait("Category", "Unit")]
    public void CtorThrowsWhenOptionsIsNull()
    {
        // Arrange
        IOptions<NoteValidationOptions>? options = null;

        // Act
        var action = () => new NoteValidator(options!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Validate rejects empty or whitespace note")]
    [Trait("Category", "Unit")]
    public void ValidateRejectsEmptyNote()
    {
        // Arrange
        var validator = new NoteValidator(Options.Create(new NoteValidationOptions()));

        // Act
        var nullResult = validator.Validate(null);
        var emptyResult = validator.Validate(string.Empty);
        var whitespaceResult = validator.Validate("   ");

        // Assert
        nullResult.IsValid.Should().BeFalse();
        nullResult.Error.Should().Be("Note must not be empty.");
        nullResult.NormalizedNote.Should().BeNull();

        emptyResult.IsValid.Should().BeFalse();
        emptyResult.Error.Should().Be("Note must not be empty.");
        emptyResult.NormalizedNote.Should().BeNull();

        whitespaceResult.IsValid.Should().BeFalse();
        whitespaceResult.Error.Should().Be("Note must not be empty.");
        whitespaceResult.NormalizedNote.Should().BeNull();
    }

    [Fact(DisplayName = "Validate trims and returns normalized note")]
    [Trait("Category", "Unit")]
    public void ValidateTrimsAndNormalizesNote()
    {
        // Arrange
        var validator = new NoteValidator(Options.Create(new NoteValidationOptions
        {
            MaxNoteLength = 20
        }));

        // Act
        var result = validator.Validate("  secret note  ");

        // Assert
        result.IsValid.Should().BeTrue();
        result.Error.Should().BeNull();
        result.NormalizedNote.Should().Be("secret note");
    }

    [Fact(DisplayName = "Validate rejects note longer than max length")]
    [Trait("Category", "Unit")]
    public void ValidateRejectsNoteLongerThanMaxLength()
    {
        // Arrange
        var validator = new NoteValidator(Options.Create(new NoteValidationOptions
        {
            MaxNoteLength = 5
        }));

        // Act
        var result = validator.Validate("123456");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Be("Note must not be longer than 5 characters.");
        result.NormalizedNote.Should().BeNull();
    }

    [Fact(DisplayName = "Validate allows note when max length is not configured")]
    [Trait("Category", "Unit")]
    public void ValidateAllowsNoteWhenNoMaxLength()
    {
        // Arrange
        var validator = new NoteValidator(Options.Create(new NoteValidationOptions
        {
            MaxNoteLength = null
        }));

        // Act
        var result = validator.Validate("1234567890");

        // Assert
        result.IsValid.Should().BeTrue();
        result.Error.Should().BeNull();
        result.NormalizedNote.Should().Be("1234567890");
    }
}
