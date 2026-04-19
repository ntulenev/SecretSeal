using FluentAssertions;

using System.Text.Json;

namespace Transport.Tests;

public sealed class RetentionPolicyResponseTests
{
    [Fact(DisplayName = "Serialize produces camelCase daysToKeep property for FE")]
    [Trait("Category", "Unit")]
    public void SerializeUsesCamelCaseProperty()
    {
        // Arrange
        var response = new RetentionPolicyResponse(14);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Act
        var json = JsonSerializer.Serialize(response, options);

        // Assert
        json.Should().Be(/*lang=json,strict*/ "{\"daysToKeep\":14}");
    }
}
