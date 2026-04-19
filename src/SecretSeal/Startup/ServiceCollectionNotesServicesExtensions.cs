using System.Diagnostics;

using Abstractions;

using Cryptography;

using Logic;

using Microsoft.EntityFrameworkCore;

using Models;

using SecretSeal.Configuration;
using SecretSeal.Services;

using Storage;
using Storage.Repositories;

using Transport.Validation;

namespace SecretSeal.Startup;

internal static class ServiceCollectionNotesServicesExtensions
{
    public static IServiceCollection AddSecretSealApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        _ = services.AddSingleton<ICryptoHelper, CryptoHelper>();
        _ = services.AddSingleton<INoteValidator, NoteValidator>();

        var storageOptions = configuration
            .GetSection("Storage")
            .Get<StorageOptions>()
            ?? throw new InvalidOperationException("Storage options are not configured.");

        return storageOptions.Mode switch
        {
            StorageMode.InMemory => services.AddInMemoryNotesStorage(),
            StorageMode.Database => services.AddDatabaseNotesStorage(configuration),
            _ => throw new UnreachableException($"Unreachable case for storage {storageOptions.Mode} mode")
        };
    }

    private static IServiceCollection AddInMemoryNotesStorage(this IServiceCollection services)
    {
        _ = services.AddSingleton<INotesHandler, InMemoryNotesHandler>();
        _ = services.Decorate<INotesHandler, CryptoNotesHandler>();
        return services;
    }

    private static IServiceCollection AddDatabaseNotesStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SecretSealDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:SecretSealDb is not configured");
        }

        _ = services.AddScoped<INotesCleaner, NotesCleaner>();
        _ = services.AddSingleton<INotesCleaningHandler, NotesCleaningHandler>();
        _ = services.AddHostedService<NotesCleanerService>();
        _ = services.AddDbContext<SecretSealDbContext>(o => o.UseSqlServer(connectionString));
        _ = services.AddScoped<IRepository<Note, NoteId>, NotesRepository>();
        _ = services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        _ = services.AddScoped<INotesHandler, StorageNotesHandler>();
        _ = services.Decorate<INotesHandler, CryptoNotesHandler>();

        return services;
    }
}
