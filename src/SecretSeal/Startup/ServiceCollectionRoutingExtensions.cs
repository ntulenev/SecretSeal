using SecretSeal.Routing;

using Transport.Serialization;

namespace SecretSeal.Startup;

/// <summary>
/// Provides service registration extensions for routing and JSON serialization.
/// </summary>
internal static class ServiceCollectionRoutingExtensions
{
    /// <summary>
    /// Registers routing conventions and JSON converters used by the API.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddSecretSealRouting(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.Configure<RouteOptions>(o => o.ConstraintMap["ShortGuid"] = typeof(ShortGuidRouteConstraint));
        _ = services.ConfigureHttpJsonOptions(
            o => o.SerializerOptions.Converters.Add(new ShortGuidJsonConverter()));

        return services;
    }
}
