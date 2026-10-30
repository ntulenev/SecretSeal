using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;

using Abstractions;

using FluentAssertions;

using Logic;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Transport;

namespace SecretSeal.Tests;

public sealed class SecretSealApiTests
{
    [Theory(DisplayName = "The home and recipient pages serve their browser scripts")]
    [InlineData("/", "Create a secret", "/js/index.js")]
    [InlineData("/read.html", "Open & destroy", "/js/read.js")]
    [Trait("Category", "Integration")]
    public async Task PagesServeTheirBrowserScripts(string path, string expectedContent, string scriptPath)
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var page = await client.GetAsync(path);
        var html = await page.Content.ReadAsStringAsync();
        var script = await client.GetAsync(scriptPath);

        page.StatusCode.Should().Be(HttpStatusCode.OK);
        page.Content.Headers.ContentType!.MediaType.Should().Be("text/html");
        html.Should().Contain(expectedContent);
        script.StatusCode.Should().Be(HttpStatusCode.OK);
        script.Content.Headers.ContentType!.MediaType.Should().Contain("javascript");
    }

    [Fact(DisplayName = "A secret created by one client can be consumed only once by another")]
    [Trait("Category", "Integration")]
    public async Task SeparateClientsShareOneTimeSecret()
    {
        using var factory = CreateFactory();
        using var sender = factory.CreateClient();
        using var recipient = factory.CreateClient();
        const string note = "private message";

        var createResponse = await sender.PostAsJsonAsync("/notes", new { note });
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var noteId = JsonSerializer.Deserialize<JsonElement>(createBody).GetProperty("id").GetString();

        var consumeResponse = await recipient.DeleteAsync($"/notes/{noteId}");
        var consumePayload = await consumeResponse.Content.ReadFromJsonAsync<JsonElement>();
        var repeatedResponse = await sender.DeleteAsync($"/notes/{noteId}");

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        createBody.Should().NotContain(note);
        consumeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        consumePayload.GetProperty("note").GetString().Should().Be(note);
        repeatedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "GET /version returns the running application's release version without caching")]
    [Trait("Category", "Integration")]
    public async Task GetVersionReturnsAssemblyInformationalVersion()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/version");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var expected = typeof(Program).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        payload.GetProperty("version").GetString().Should().Be(expected);
        response.Headers.CacheControl!.NoStore.Should().BeTrue();
    }

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
        payload.GetProperty("error").GetString().Should().Be("This note is too large.");
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

    [Fact(DisplayName = "DELETE /notes/{id} consumes note only once")]
    [Trait("Category", "Integration")]
    public async Task DeleteWhenNoteIsAlreadyConsumedReturnsNotFound()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var note = "one time";

        var createResponse = await client.PostAsJsonAsync("/notes", new { note });
        var createPayload = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var noteId = createPayload.GetProperty("id").GetString();

        // Act
        var firstDeleteResponse = await client.DeleteAsync($"/notes/{noteId}");
        var secondDeleteResponse = await client.DeleteAsync($"/notes/{noteId}");
        var secondDeletePayload = await secondDeleteResponse.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstDeleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        secondDeletePayload.GetProperty("error").GetString().Should().Be("Note not found (or already consumed).");
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

    [Fact(DisplayName = "DELETE /notes/{id} rejects invalid route id")]
    [Trait("Category", "Integration")]
    public async Task DeleteWhenRouteIdIsInvalidReturnsNotFound()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("/notes/not-a-guid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<INotesHandler>();
                _ = services.AddSingleton<INotesHandler>(serviceProvider =>
                    new CryptoNotesHandler(
                        new InMemoryNotesHandler(),
                        serviceProvider.GetRequiredService<ICryptoHelper>()));
            });
        }
    }
}
