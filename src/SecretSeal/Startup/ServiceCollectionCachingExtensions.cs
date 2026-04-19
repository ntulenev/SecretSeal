namespace SecretSeal.Startup;

internal static class ServiceCollectionCachingExtensions
{
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
