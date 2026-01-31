using FluentAssertions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Storage;

namespace SecretSeal.Tests;

public sealed class RetentionPolicyTests
{
    [Fact(DisplayName = "GET /retention-policy returns -1 in InMemory mode")]
    [Trait("Category", "Integration")]
    public async Task GetRetentionPolicyInMemoryReturnsMinusOne()
    {
        // Arrange
        using var factory = CreateFactory(storageMode: "InMemory", daysToKeep: "30");
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/retention-policy");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        payload.GetProperty("daysToKeep").GetInt32().Should().Be(-1);
    }

    [Fact(DisplayName = "GET /retention-policy returns configured days in Database mode")]
    [Trait("Category", "Integration")]
    public async Task GetRetentionPolicyDatabaseReturnsConfiguredDays()
    {
        // Arrange
        const string daysToKeep = "14";
        using var factory = CreateFactory(storageMode: "Database", daysToKeep);
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/retention-policy");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        payload.GetProperty("daysToKeep").GetInt32().Should().Be(14);
    }

    private static SecretSealAppFactory CreateFactory(string storageMode, string daysToKeep) =>
        new(storageMode, daysToKeep);

    private sealed class SecretSealAppFactory : WebApplicationFactory<Program>
    {
        private readonly string _storageMode;
        private readonly string _daysToKeep;

        public SecretSealAppFactory(string storageMode, string daysToKeep)
        {
            _storageMode = storageMode;
            _daysToKeep = daysToKeep;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                var settings = new Dictionary<string, string?>
                {
                    ["Crypto:Key"] = "12345678901234567890123456789012",
                    ["Storage:Mode"] = _storageMode,
                    ["Validation:MaxNoteLength"] = "15",
                    ["NotesCleaner:DaysToKeep"] = _daysToKeep,
                    ["NotesCleaner:CleanupInterval"] = "01:00:00",
                    ["ConnectionStrings:SecretSealDb"] =
                        "Server=(localdb)\\MSSQLLocalDB;Database=SecretSealTests;Trusted_Connection=True;"
                };

                config.AddInMemoryCollection(settings);
            });

            if (_storageMode == "Database")
            {
                builder.ConfigureServices(services =>
                {
                    _ = services.AddDbContext<SecretSealDbContext>(options =>
                        options.UseInMemoryDatabase("SecretSealTests"));
                });
            }
        }
    }
}
