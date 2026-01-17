using FluentAssertions;

using System.Text.Json;

namespace Transport.Tests;

public sealed class CreateNoteResponseTests
{
    [Fact(DisplayName = "Serialize produces camelCase id property for FE")]
    [Trait("Category", "Unit")]
    public void SerializeUsesCamelCaseIdProperty()
    {
        //Arrage
        var id = Guid.NewGuid();
        var response = new CreateNoteResponse(id);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        //Act
        var json = JsonSerializer.Serialize(response, options);

        //Assert
        json.Should().Be($"{{\"id\":\"{id}\"}}");
    }
}
