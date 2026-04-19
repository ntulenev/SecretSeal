using SecretSeal.UseCases;

using Transport;

namespace SecretSeal.Endpoints;

internal static class WebApplicationEndpointExtensions
{
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
