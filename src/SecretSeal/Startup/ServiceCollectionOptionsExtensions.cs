using Cryptography.Configuration;

using Logic.Configuration;

using SecretSeal.Configuration;

using Transport.Configuration;

namespace SecretSeal.Startup;

internal static class ServiceCollectionOptionsExtensions
{
    public static IServiceCollection AddSecretSealOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        _ = services
            .AddOptions<StorageOptions>()
            .Bind(configuration.GetSection("Storage"))
            .ValidateOnStart();

        _ = services
            .AddOptions<NoteValidationOptions>()
            .Bind(configuration.GetSection("Validation"))
            .Validate(
                o => o.MaxNoteLength is null or > 0,
                "Validation:MaxNoteLength must be a positive integer.")
            .ValidateOnStart();

        _ = services
            .AddOptions<CryptoOptions>()
            .Bind(configuration.GetSection("Crypto"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        _ = services
            .AddOptions<NotesCleanerOptions>()
            .Bind(configuration.GetSection("NotesCleaner"))
            .ValidateDataAnnotations()
            .Validate(
                o => o.CleanupInterval > TimeSpan.Zero,
                "NotesCleaner:CleanupInterval must be greater than zero.")
            .ValidateOnStart();

        return services;
    }
}
