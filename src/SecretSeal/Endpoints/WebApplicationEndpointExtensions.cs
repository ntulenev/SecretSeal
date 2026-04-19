using SecretSeal.UseCases;

using Transport;

namespace SecretSeal.Endpoints;

/// <summary>
/// Provides endpoint mapping extensions for the SecretSeal web application.
/// </summary>
internal static class WebApplicationEndpointExtensions
{
    /// <summary>
    /// Maps the SecretSeal HTTP endpoints to the application.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> instance for chaining.</returns>
    public static WebApplication MapSecretSealEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        _ = app.MapPost(
            "/notes",
            async (CreateNoteRequest request, CreateNoteUseCase useCase, CancellationToken cancellationToken) =>
            {
                var result = await useCase.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
                return result.IsSuccess
                    ? Results.Ok(result.Response)
                    : Results.BadRequest(new { error = result.Error });
            });

        _ = app.MapDelete(
            "/notes/{id:ShortGuid}",
            async (ShortGuid id, TakeNoteUseCase useCase, CancellationToken cancellationToken) =>
            {
                var response = await useCase.ExecuteAsync(id, cancellationToken).ConfigureAwait(false);
                return response is null
                    ? Results.NotFound(new { error = "Note not found (or already consumed)." })
                    : Results.Ok(response);
            });

        _ = app.MapGet("/hc", () => Results.Ok(new { status = "healthy" }));

        _ = app.MapGet(
                "/stat",
                async (GetNoteStatsUseCase useCase, CancellationToken cancellationToken) =>
                    Results.Ok(await useCase.ExecuteAsync(cancellationToken).ConfigureAwait(false)))
            .CacheOutput("stat-1m");

        _ = app.MapGet(
                "/retention-policy",
                (GetRetentionPolicyUseCase useCase) => Results.Ok(useCase.Execute()))
            .CacheOutput("retention-24h");

        return app;
    }
}
