using Abstractions;

using Logic;

using Microsoft.Extensions.Options;

using Models;

using SecretSeal.Configuration;
using SecretSeal.Startup;

using Transport;
using Transport.Validation;

using var app = StartupHelpers.CreateApplication(args);
app.UseOutputCache();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/notes",
    async (CreateNoteRequest req,
           INoteValidator validator,
           INotesHandler handler,
           CancellationToken token) =>
    {
        var result = validator.Validate(req.Note);

        if (!result.IsValid)
        {
            return Results.BadRequest(new { error = result.Error });
        }

        var internalNote = Note.Create(result.NormalizedNote!);
        await handler.AddNoteAsync(internalNote, token).ConfigureAwait(false);
        return Results.Ok(new CreateNoteResponse(internalNote.Id.Value));
    });

app.MapDelete("/notes/{id:ShortGuid}", async (ShortGuid id, INotesHandler handler, CancellationToken token) =>
{
    var noteId = new NoteId(id);
    var note = await handler.TakeNoteAsync(noteId, token).ConfigureAwait(false);
    if (note is null)
    {
        return Results.NotFound(new { error = "Note not found (or already consumed)." });
    }
    return Results.Ok(new { id, note = note.Content });
});

app.MapGet("/hc", () => Results.Ok(new { status = "healthy" }));

app.MapGet("/stat", async (INotesHandler handler, CancellationToken token, IOptions<StorageOptions> storageMode) =>
{
    var count = await handler.GetNotesCountAsync(token).ConfigureAwait(false);
    var encryptionEnabled = handler is CryptoNotesHandler;
    var isInMemory = storageMode.Value.Mode == StorageMode.InMemory;
    return Results.Ok(new StatResponse(count, encryptionEnabled, isInMemory));
})
.CacheOutput("stat-1m");

await StartupHelpers.RunAppAsync(app).ConfigureAwait(false);

