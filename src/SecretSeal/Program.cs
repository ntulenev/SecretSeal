using Abstractions;

using Logic;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Models;

using SecretSeal.Configuration;
using SecretSeal.Startup;

using Transport;

using var app = StartupHelpers.CreateApplication(args);

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/notes",
    async (CreateNoteRequest req,
           IOptions<StorageOptions> storageOptions,
           INotesHandler handler,
           CancellationToken token) =>
    {
        var note = req.Note?.Trim();
        if (string.IsNullOrEmpty(note))
        {
            return Results.BadRequest(new { error = "Note must not be empty." });
        }

        var max = storageOptions.Value.MaxNoteLength;
        if (max is not null && note.Length > max.Value)
        {
            return Results.BadRequest(new { error = $"Note must not be longer than {max.Value} characters." });
        }

        var internalNote = Note.Create(note);
        await handler.AddNoteAsync(internalNote, token).ConfigureAwait(false);
        return Results.Ok(new CreateNoteResponse(internalNote.Id.Value));
    });

app.MapDelete("/notes/{id:guid}", async (Guid id, INotesHandler handler, CancellationToken token) =>
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
});

await StartupHelpers.RunAppAsync(app).ConfigureAwait(false);

