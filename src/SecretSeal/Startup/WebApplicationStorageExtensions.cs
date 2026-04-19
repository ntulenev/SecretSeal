using SecretSeal.Configuration;

using Storage;

namespace SecretSeal.Startup;

/// <summary>
/// Provides storage-related startup extensions for the web application.
/// </summary>
internal static class WebApplicationStorageExtensions
{
    /// <summary>
    /// Ensures persistent storage is initialized when the application runs in database mode.
    /// </summary>
    /// <param name="app">The web application whose configured storage should be initialized.</param>
    public static async Task EnsureStorageCreatedIfNeededAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var storageOptions = app.Configuration
            .GetSection("Storage")
            .Get<StorageOptions>()
            ?? throw new InvalidOperationException("Storage options are not configured.");

        if (storageOptions.Mode != StorageMode.Database)
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SecretSealDbContext>();
        _ = await db.Database.EnsureCreatedAsync().ConfigureAwait(false);
    }
}
