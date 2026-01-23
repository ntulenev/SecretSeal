using FluentAssertions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Transport;

namespace SecretSeal.Tests;

public sealed class SecretSealApiTests
{
    [Fact(DisplayName = "GET /hc returns healthy status")]
    [Trait("Category", "Integration")]
    public async Task GetHealthCheckReturnsHealthyStatus()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/hc");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        payload.GetProperty("status").GetString().Should().Be("healthy");
    }

    [Fact(DisplayName = "POST /notes rejects empty note")]
    [Trait("Category", "Integration")]
    public async Task CreateNoteWhenNoteIsEmptyReturnsBadRequest()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/notes", new { note = "   " });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        payload.GetProperty("error").GetString().Should().Be("Note must not be empty.");
    }

    [Fact(DisplayName = "POST /notes rejects long note")]
    [Trait("Category", "Integration")]
    public async Task CreateNoteWhenNoteIsBigReturnsBadRequest()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/notes", new { note = "0123456789ABCDFX" });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        payload.GetProperty("error").GetString().Should().Be("Note must not be longer than 15 characters.");
    }

    [Fact(DisplayName = "POST /notes returns id and DELETE /notes/{id} returns note content")]
    [Trait("Category", "Integration")]
    public async Task CreateThenDeleteReturnsNoteContent()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var note = "keep it safe";

        // Act
        var createResponse = await client.PostAsJsonAsync("/notes", new { note });
        var createPayload = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var noteId = createPayload.GetProperty("id").GetString();

        var deleteResponse = await client.DeleteAsync($"/notes/{noteId}");
        var deletePayload = await deleteResponse.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        noteId.Should().NotBe(string.Empty);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        deletePayload.GetProperty("id").GetString().Should().Be(noteId);
        deletePayload.GetProperty("note").GetString().Should().Be(note);
    }

    [Fact(DisplayName = "DELETE /notes/{id} returns not found for missing note")]
    [Trait("Category", "Integration")]
    public async Task DeleteWhenNoteDoesNotExistReturnsNotFound()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var noteId = ShortGuid.FromGuid(Guid.NewGuid()).ToString();

        // Act
        var response = await client.DeleteAsync($"/notes/{noteId}");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        payload.GetProperty("error").GetString().Should().Be("Note not found (or already consumed).");
    }

    [Fact(DisplayName = "GET /stat returns note count and encryption flag")]
    [Trait("Category", "Integration")]
    public async Task GetStatsReturnsCountAndEncryptionFlag()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync("/notes", new { note = "one" });
        await client.PostAsJsonAsync("/notes", new { note = "two" });

        // Act
        var response = await client.GetAsync("/stat");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        payload.GetProperty("notesCount").GetInt64().Should().Be(2);
        payload.GetProperty("encryptionEnabled").GetBoolean().Should().BeTrue();
    }

    private static SecretSealAppFactory CreateFactory() => new();

    private sealed class SecretSealAppFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                var settings = new Dictionary<string, string?>
                {
                    ["Crypto:Key"] = "12345678901234567890123456789012",
                    ["Storage:Mode"] = "InMemory",
                    ["Validation:MaxNoteLength"] = "15"
                };

                config.AddInMemoryCollection(settings);
            });
        }
    }
}
