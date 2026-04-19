using FluentAssertions;

using System.Text.Json;

using Transport.Serialization;

namespace Transport.Tests;

public sealed class TakeNoteResponseTests
{
    [Fact(DisplayName = "Serialize produces camelCase properties for FE")]
    [Trait("Category", "Unit")]
    public void SerializeUsesCamelCaseProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = new TakeNoteResponse(id, "secret");
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        options.Converters.Add(new ShortGuidJsonConverter());

        // Act
        var json = JsonSerializer.Serialize(response, options);

        // Assert
        json.Should().Be($"{{\"id\":\"{new ShortGuid(id)}\",\"note\":\"secret\"}}");
    }
}
