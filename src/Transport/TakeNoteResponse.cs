namespace Transport;

/// <summary>
/// Represents the result of consuming a note.
/// </summary>
/// <param name="Id">The unique identifier of the consumed note.</param>
/// <param name="Note">The note content.</param>
public sealed record TakeNoteResponse(ShortGuid Id, string Note);
