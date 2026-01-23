using System.Text.Json;
using System.Text.Json.Serialization;

using FluentAssertions;

using Transport.Serialization;

namespace Transport.Tests;

public sealed class ShortGuidJsonConverterTests
{
    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ShortGuidJsonConverter());
        return options;
    }

    [Fact(DisplayName = "Write serializes short guid as JSON string value")]
    [Trait("Category", "Unit")]
    public void WriteSerializesAsJsonString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var shortGuid = new ShortGuid(guid);
        var options = CreateOptions();

        // Act
        var json = JsonSerializer.Serialize(shortGuid, options);

        // Assert
        json.Should().Be($"\"{shortGuid}\"");
    }

    [Fact(DisplayName = "Read deserializes from short guid string")]
    [Trait("Category", "Unit")]
    public void ReadDeserializesFromShortGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var original = new ShortGuid(guid);
        var json = $"\"{original}\"";
        var options = CreateOptions();

        // Act
        var parsed = JsonSerializer.Deserialize<ShortGuid>(json, options);

        // Assert
        parsed.Value.Should().Be(guid);
        parsed.ToString().Should().Be(original.ToString());
    }

    [Fact(DisplayName = "Read deserializes from standard guid string")]
    [Trait("Category", "Unit")]
    public void ReadDeserializesFromStandardGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var json = $"\"{guid:D}\"";
        var options = CreateOptions();

        // Act
        var parsed = JsonSerializer.Deserialize<ShortGuid>(json, options);

        // Assert
        parsed.Value.Should().Be(guid);
        parsed.ToString().Length.Should().Be(22);
    }

    [Theory(DisplayName = "Read throws JsonException when token is not a string")]
    [Trait("Category", "Unit")]
    [InlineData("null")]
    [InlineData("123")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("[]")]
    [InlineData("{}")]
    public void ReadThrowsWhenTokenIsNotString(string json)
    {
        // Arrange
        var options = CreateOptions();

        // Act
        var act = () => JsonSerializer.Deserialize<ShortGuid>(json, options);

        // Assert
        act.Should()
            .Throw<JsonException>()
            .WithMessage("ShortGuid must be represented as a JSON string.");
    }

    [Theory(DisplayName = "Read throws JsonException when string value cannot be parsed")]
    [Trait("Category", "Unit")]
    [InlineData("\"\"")]
    [InlineData("\" \"")]
    [InlineData("\"not-a-guid\"")]
    [InlineData("\"00000000-0000-0000-0000-00000000000Z\"")]
    [InlineData("\"00000000-0000-0000-0000-0000000000000\"")]
    public void ReadThrowsWhenValueIsInvalid(string json)
    {
        // Arrange
        var options = CreateOptions();

        // Act
        var act = () => JsonSerializer.Deserialize<ShortGuid>(json, options);

        // Assert
        act.Should()
            .Throw<JsonException>()
            .WithMessage("Invalid ShortGuid value: *");
    }

    [Fact(DisplayName = "Write throws when writer is null")]
    [Trait("Category", "Unit")]
    public void WriteThrowsWhenWriterIsNull()
    {
        // Arrange
        var converter = new ShortGuidJsonConverter();
        Utf8JsonWriter? writer = null;
        var value = new ShortGuid(Guid.NewGuid());
        var options = new JsonSerializerOptions();

        // Act
        var act = () => converter.Write(writer!, value, options);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Roundtrip serialize/deserialize preserves underlying guid")]
    [Trait("Category", "Unit")]
    public void RoundtripPreservesGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var original = new ShortGuid(guid);
        var options = CreateOptions();

        // Act
        var json = JsonSerializer.Serialize(original, options);
        var parsed = JsonSerializer.Deserialize<ShortGuid>(json, options);

        // Assert
        parsed.Value.Should().Be(guid);
        parsed.ToString().Should().Be(original.ToString());
    }

    [Fact(DisplayName = "Converter can be used on DTO property")]
    [Trait("Category", "Unit")]
    public void ConverterWorksOnDtoProperty()
    {
        // Arrange
        var options = CreateOptions();
        var guid = Guid.NewGuid();
        var dto = new TestDto { Id = new ShortGuid(guid) };

        // Act
        var json = JsonSerializer.Serialize(dto, options);
        var parsed = JsonSerializer.Deserialize<TestDto>(json, options);

        // Assert
        json.Should().Be($"{{\"id\":\"{dto.Id}\"}}");
        parsed.Should().NotBeNull();
        parsed!.Id.Value.Should().Be(guid);
    }

    [Fact(DisplayName = "Write produces valid JSON without extra whitespace")]
    [Trait("Category", "Unit")]
    public void WriteProducesCompactJsonString()
    {
        // Arrange
        var options = CreateOptions();
        var value = new ShortGuid(Guid.NewGuid());

        // Act
        var json = JsonSerializer.Serialize(value, options);

        // Assert
        json.Should().StartWith("\"");
        json.Should().EndWith("\"");
        json.Should().NotContain(" ");
        json.Length.Should().Be(24); // quotes + 22 chars
    }

    private sealed class TestDto
    {
        [JsonPropertyName("id")]
        public ShortGuid Id { get; set; }
    }
}
