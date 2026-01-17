using Abstractions;
using Cryptography;
using Cryptography.Configuration;
using Logic;
using Models;
using Transport;

var builder = WebApplication.CreateBuilder(args);
_ = builder.Services.AddSingleton<ICryptoHelper, CryptoHelper>();
_ = builder.Services.AddSingleton<INotesHandler, CryptoNotesHandler>();
_ = builder.Services.AddSingleton<INotesHandler, InMemoryNotesHandler>();
_ = builder.Services.Decorate<INotesHandler, CryptoNotesHandler>();
_ = builder.Services
    .AddOptions<CryptoOptions>()
    .Bind(builder.Configuration.GetSection("Crypto"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/notes",
    async (CreateNoteRequest req, INotesHandler handler, CancellationToken token) =>
    {
        var note = (req.Note ?? string.Empty).Trim();
        if (note.Length == 0)
        {
            return Results.BadRequest(new { error = "Note must not be empty." });
        }
        var internalNote = Note.Create(note);
        await handler.AddNoteAsync(Note.Create(note), token).ConfigureAwait(false);
        return Results.Ok(new CreateNoteResponse(internalNote.Id.Value));
    });
app.MapDelete("/notes/{id:guid}", async (Guid id, INotesHandler handler, CancellationToken token) =>
{
    var noteId = new NoteId(id);
    var note = await handler.TakeNoteAsync(noteId, token);
    if (note is null)
    {
        return Results.NotFound(new { error = "Note not found (or already consumed)." });
    }
    return Results.Ok(new { id, note = note.Content });
});
app.MapGet("/hc", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/stat", async (INotesHandler handler, CancellationToken token) =>
{
    var count = await handler.GetNotesCountAsync(token);
    var encryptionEnabled = handler is CryptoNotesHandler;
    return Results.Ok(new StatResponse(count, encryptionEnabled));
});

await app.RunAsync().ConfigureAwait(false);