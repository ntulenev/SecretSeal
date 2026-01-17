using FluentAssertions;

using System.Text.Json;

namespace Transport.Tests;

public sealed class StatResponseTests
{
    [Fact(DisplayName = "Serialize produces camelCase properties for FE")]
    [Trait("Category", "Unit")]
    public void SerializeUsesCamelCaseProperties()
    {
        //Arrage
        const long notesCount = 42;
        const bool encryptionEnabled = true;
        const bool inMemory = true;
        var response = new StatResponse(notesCount, encryptionEnabled, inMemory);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        //Act
        var json = JsonSerializer.Serialize(response, options);

        //Assert
        json.Should().Be(/*lang=json,strict*/ "{\"notesCount\":42,\"encryptionEnabled\":true,\"isInMemory\":true}");
    }
}