using FluentAssertions;

using Transport.Validation;

namespace Transport.Tests;

public sealed class NoteValidationResultTests
{
    [Fact(DisplayName = "Success creates valid result with normalized note")]
    [Trait("Category", "Unit")]
    public void SuccessCreatesValidResult()
    {
        // Arrange
        var note = "normalized";

        // Act
        var result = NoteValidationResult.Success(note);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Error.Should().BeNull();
        result.NormalizedNote.Should().Be(note);
    }

    [Fact(DisplayName = "Fail creates invalid result with error")]
    [Trait("Category", "Unit")]
    public void FailCreatesInvalidResult()
    {
        // Arrange
        var error = "bad note";

        // Act
        var result = NoteValidationResult.Fail(error);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Be(error);
        result.NormalizedNote.Should().BeNull();
    }
}
