using SecretSeal.Routing;

using Transport.Serialization;

namespace SecretSeal.Startup;

internal static class ServiceCollectionRoutingExtensions
{
    public static IServiceCollection AddSecretSealRouting(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.Configure<RouteOptions>(o => o.ConstraintMap["ShortGuid"] = typeof(ShortGuidRouteConstraint));
        _ = services.ConfigureHttpJsonOptions(
            o => o.SerializerOptions.Converters.Add(new ShortGuidJsonConverter()));

        return services;
    }
}
