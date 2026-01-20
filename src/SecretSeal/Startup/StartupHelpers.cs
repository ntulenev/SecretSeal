using System.Diagnostics;

using Abstractions;

using Cryptography;
using Cryptography.Configuration;

using Logic;

using Microsoft.EntityFrameworkCore;

using Models;

using SecretSeal.Configuration;
using SecretSeal.Validation;

using Storage;
using Storage.Repositories;

namespace SecretSeal.Startup;

/// <summary>
/// Provides helper methods for application startup.
/// </summary>
internal static class StartupHelpers
{
    /// <summary>
    /// Builds and configures the <see cref="WebApplication"/> and its services.
    /// </summary>
    /// <param name="args">The application command-line arguments.</param>
    /// <returns>The configured <see cref="WebApplication"/> instance.</returns>
    public static WebApplication CreateApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureOptions(builder);
        RegisterServices(builder);
        ConfigureCache(builder);

        return builder.Build();
    }

    /// <summary>
    /// Runs the application and ensures the database is created when using database storage.
    /// </summary>
    /// <param name="app">The configured <see cref="WebApplication"/> instance.</param>
    /// <returns>A task that represents the asynchronous run operation.</returns>
    public static async Task RunAppAsync(WebApplication app)
    {
        await EnsureDatabaseCreatedIfNeededAsync(app).ConfigureAwait(false);
        await app.RunAsync().ConfigureAwait(false);
    }

    private static void ConfigureOptions(WebApplicationBuilder builder)
    {
        _ = builder.Services
            .AddOptions<StorageOptions>()
            .Bind(builder.Configuration.GetSection("Storage"))
            .Validate(
                o => o.MaxNoteLength is null or > 0,
                "Storage:MaxNoteLength must be a positive integer.")
            .ValidateOnStart();

        _ = builder.Services
            .AddOptions<CryptoOptions>()
            .Bind(builder.Configuration.GetSection("Crypto"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    private static void RegisterServices(WebApplicationBuilder builder)
    {
        _ = builder.Services.AddSingleton<ICryptoHelper, CryptoHelper>();
        _ = builder.Services.AddSingleton<INoteValidator, NoteValidator>();

        var storageOption = builder.Configuration
            .GetSection("Storage")
            .Get<StorageOptions>()
            ?? throw new InvalidOperationException("Storage options are not configured.");

        switch (storageOption.Mode)
        {
            case StorageMode.InMemory:
                _ = builder.Services.AddSingleton<INotesHandler, CryptoNotesHandler>();
                _ = builder.Services.AddSingleton<INotesHandler, InMemoryNotesHandler>();
                break;

            case StorageMode.Database:
                _ = builder.Services.AddScoped<INotesHandler, CryptoNotesHandler>();

                var cs = builder.Configuration.GetConnectionString("SecretSealDb");
                if (string.IsNullOrWhiteSpace(cs))
                {
                    throw new InvalidOperationException("ConnectionStrings:SecretSealDb is not configured");
                }

                _ = builder.Services.AddDbContext<SecretSealDbContext>(o => o.UseSqlServer(cs));
                _ = builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
                _ = builder.Services.AddScoped<IRepository<Note, NoteId>, NotesRepository>();
                _ = builder.Services.AddScoped<INotesHandler, StorageNotesHandler>();
                break;

            default:
                throw new UnreachableException(
                    $"Unreachable case for storage {storageOption.Mode} mode");
        }

        _ = builder.Services.Decorate<INotesHandler, CryptoNotesHandler>();
    }

    private static void ConfigureCache(WebApplicationBuilder builder)
    {
        _ = builder.Services.AddOutputCache(options =>
            options.AddPolicy("stat-1m", p => p.Expire(TimeSpan.FromMinutes(1))));
    }

    private static async Task EnsureDatabaseCreatedIfNeededAsync(WebApplication app)
    {
        var storageOption = app.Configuration
            .GetSection("Storage")
            .Get<StorageOptions>()
            ?? throw new InvalidOperationException("Storage options are not configured.");

        if (storageOption.Mode == StorageMode.Database)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SecretSealDbContext>();
            _ = await db.Database.EnsureCreatedAsync().ConfigureAwait(false);
        }
    }
}
