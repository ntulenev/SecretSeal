using SecretSeal.Configuration;

using Storage;

namespace SecretSeal.Startup;

internal static class WebApplicationStorageExtensions
{
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
