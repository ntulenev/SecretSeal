namespace Transport;

/// <summary>
/// Represents the result of a create note operation, including the unique identifier of the newly created note.
/// </summary>
/// <param name="Id">The unique identifier assigned to the newly created note.</param>
public sealed record CreateNoteResponse(ShortGuid Id);
