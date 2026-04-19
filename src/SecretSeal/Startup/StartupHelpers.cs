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

        _ = builder.Services.AddSecretSealOptions(builder.Configuration);
        _ = builder.Services.AddSecretSealRouting();
        _ = builder.Services.AddSecretSealApplicationServices(builder.Configuration);
        _ = builder.Services.AddSecretSealCaching();

        return builder.Build();
    }

    /// <summary>
    /// Runs the application and ensures the database is created when using database storage.
    /// </summary>
    /// <param name="app">The configured <see cref="WebApplication"/> instance.</param>
    /// <returns>A task that represents the asynchronous run operation.</returns>
    public static async Task RunAppAsync(WebApplication app)
    {
        await app.EnsureStorageCreatedIfNeededAsync().ConfigureAwait(false);
        await app.RunAsync().ConfigureAwait(false);
    }
}
