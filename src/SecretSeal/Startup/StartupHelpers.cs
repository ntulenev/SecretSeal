using System.Diagnostics;

using Abstractions;

using Cryptography;
using Cryptography.Configuration;

using Logic;

using Microsoft.EntityFrameworkCore;

using Models;

using SecretSeal.Configuration;

using Storage;
using Storage.Repositories;

namespace SecretSeal.Startup;

/// <summary>
/// 
/// </summary>
internal static class StartupHelpers
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="UnreachableException"></exception>
    public static WebApplication CreateApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        _ = builder.Services.AddSingleton<ICryptoHelper, CryptoHelper>();
        _ = builder.Services
            .AddOptions<StorageOptions>()
            .Bind(builder.Configuration.GetSection("Storage"))
            .Validate(o => o.MaxNoteLength is null or > 0,
                "Storage:MaxNoteLength must be a positive integer.")
            .ValidateOnStart();
        _ = builder.Services
            .AddOptions<CryptoOptions>()
            .Bind(builder.Configuration.GetSection("Crypto"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        _ = builder.Services.AddOutputCache(options =>
                    options.AddPolicy("stat-1m", p => p.Expire(TimeSpan.FromMinutes(1))));

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
                throw new UnreachableException($"Unreachable case for storage {storageOption.Mode} mode");
        }

        _ = builder.Services.Decorate<INotesHandler, CryptoNotesHandler>();
        return builder.Build();

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task RunAppAsync(WebApplication app)
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

        await app.RunAsync().ConfigureAwait(false);
    }
}
