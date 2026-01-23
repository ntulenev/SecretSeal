using FluentAssertions;

using System.Text.Json;

namespace Transport.Tests;

public sealed class CreateNoteRequestTests
{
    [Fact(DisplayName = "Deserialize reads camelCase note property from FE")]
    [Trait("Category", "Unit")]
    public void DeserializeReadsCamelCaseNoteProperty()
    {
        // Arrange
        var json = "{\"note\":\"hello\"}";
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Act
        var result = JsonSerializer.Deserialize<CreateNoteRequest>(json, options);

        // Assert
        result.Should().NotBeNull();
        result!.Note.Should().Be("hello");
    }

    [Fact(DisplayName = "Deserialize reads null note property from FE")]
    [Trait("Category", "Unit")]
    public void DeserializeReadsNullNoteProperty()
    {
        // Arrange
        var json = "{\"note\":null}";
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Act
        var result = JsonSerializer.Deserialize<CreateNoteRequest>(json, options);

        // Assert
        result.Should().NotBeNull();
        result!.Note.Should().BeNull();
    }
}
