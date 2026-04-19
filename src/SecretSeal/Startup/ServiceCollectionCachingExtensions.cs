namespace SecretSeal.Startup;

/// <summary>
/// Provides service registration extensions for output caching.
/// </summary>
internal static class ServiceCollectionCachingExtensions
{
    /// <summary>
    /// Registers output cache policies used by the application endpoints.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddSecretSealCaching(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddOutputCache(options =>
        {
            options.AddPolicy("stat-1m", p => p.Expire(TimeSpan.FromMinutes(1)));
            options.AddPolicy("retention-24h", p => p.Expire(TimeSpan.FromHours(24)));
        });

        return services;
    }
}
