namespace Transport;

/// <summary>
/// Represents a request to create a new note with the specified content.
/// </summary>
/// <param name="Note">The text content of the note to create. Can be null or empty if no content is provided.</param>
public sealed record CreateNoteRequest(string? Note);
