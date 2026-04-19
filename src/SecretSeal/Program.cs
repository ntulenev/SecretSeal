using SecretSeal.Endpoints;
using SecretSeal.Startup;

using var app = StartupHelpers.CreateApplication(args);
app.UseOutputCache();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapSecretSealEndpoints();

await StartupHelpers.RunAppAsync(app).ConfigureAwait(false);

