using Abstractions;
using Cryptography;
using Cryptography.Configuration;
using Models;
using System.Collections.Concurrent;
using Transport;

var builder = WebApplication.CreateBuilder(args);
_ = builder.Services.AddSingleton<ICryptoHelper, CryptoHelper>();
builder.Services.Configure<CryptoOptions>(
    builder.Configuration.GetSection("Crypto"));

var app = builder.Build();

// In-memory encrypted note store (stores Base64(IV || CIPHERTEXT))
var notes = new ConcurrentDictionary<NoteId, Note>();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/notes", (CreateNoteRequest req, ICryptoHelper crypto) =>
{
    var note = (req.Note ?? string.Empty).Trim();
    if (note.Length == 0)
    {
        return Results.BadRequest(new { error = "Note must not be empty." });
    }

    var encrypted = crypto.Encrypt(note);

    var internalNote = Note.Create(encrypted);
    notes[internalNote.Id] = internalNote;

    return Results.Ok(new CreateNoteResponse(internalNote.Id.Value));
});

app.MapDelete("/notes/{id:guid}", (Guid id, ICryptoHelper crypto) =>
{
    var noteId = new NoteId(id);
    if (!notes.TryRemove(noteId, out var encrypted))
        return Results.NotFound(new { error = "Note not found (or already consumed)." });

    var decrypted = crypto.Decrypt(encrypted.Content);

    return Results.Ok(new { id, note = decrypted });
});

app.MapGet("/hc", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/stat", () => Results.Ok(new StatResponse(notes.Count)));

await app.RunAsync().ConfigureAwait(false);