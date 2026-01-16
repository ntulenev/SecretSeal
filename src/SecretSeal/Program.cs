using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// =====================
// In-memory note store
// =====================
// NOTE: This is per-process memory only. Restart => all notes are gone.
// Thread-safe for concurrent requests.
var notes = new ConcurrentDictionary<Guid, string>();


app.UseDefaultFiles();
app.UseStaticFiles();  

// =====================
// Endpoints
// =====================

// 1) Create a note -> returns GUID
app.MapPost("/notes", (CreateNoteRequest req) =>
{
    var note = (req.Note ?? string.Empty).Trim();

    if (note.Length == 0)
        return Results.BadRequest(new { error = "Note must not be empty." });

    var id = Guid.NewGuid();
    notes[id] = note;

    return Results.Ok(new CreateNoteResponse(id));
});

// 2) Read once: return note and destroy it
app.MapDelete("/notes/{id:guid}", (Guid id) =>
{
    if (!notes.TryRemove(id, out var note))
        return Results.NotFound(new { error = "Note not found (or already consumed)." });

    return Results.Ok(new { id, note });
});

// Healthcheck (always healthy for now)
app.MapGet("/hc", () => Results.Ok(new { status = "healthy" }));

// Stat: how many notes exist right now
app.MapGet("/stat", () => Results.Ok(new StatResponse(notes.Count)));

app.Run();


// =====================
// DTOs
// =====================
public sealed record CreateNoteRequest(string? Note);
public sealed record CreateNoteResponse(Guid Id);
public sealed record StatResponse(int NotesCount);