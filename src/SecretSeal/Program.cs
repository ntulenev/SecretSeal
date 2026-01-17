using Abstractions;
using Cryptography;
using Cryptography.Configuration;
using Logic;
using Models;
using Transport;

var builder = WebApplication.CreateBuilder(args);
_ = builder.Services.AddSingleton<ICryptoHelper, CryptoHelper>();
_ = builder.Services.AddSingleton<INotesHandler, InMemoryNotesHandler>();
builder.Services.Configure<CryptoOptions>(
    builder.Configuration.GetSection("Crypto"));

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/notes",
    async (CreateNoteRequest req, ICryptoHelper crypto, INotesHandler handler, CancellationToken token) =>
    {
        var note = (req.Note ?? string.Empty).Trim();
        if (note.Length == 0)
        {
            return Results.BadRequest(new { error = "Note must not be empty." });
        }

        var encrypted = crypto.Encrypt(note);

        var internalNote = Note.Create(encrypted);
        await handler.AddNoteAsync(internalNote, token).ConfigureAwait(false);
        return Results.Ok(new CreateNoteResponse(internalNote.Id.Value));
    });

app.MapDelete("/notes/{id:guid}", async (Guid id, ICryptoHelper crypto, INotesHandler handler, CancellationToken token) =>
{
    var noteId = new NoteId(id);

    if (!await handler.TryReadNoteAsync(noteId, token, out Note note))
    {
        return Results.NotFound(new { error = "Note not found (or already consumed)." });
    }

    var decrypted = crypto.Decrypt(note.Content);

    return Results.Ok(new { id, note = decrypted });
});

app.MapGet("/hc", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/stat", async (INotesHandler handler, CancellationToken token) =>
    Results.Ok(new StatResponse(await handler.GetNotesCountAsync(token))));

await app.RunAsync().ConfigureAwait(false);