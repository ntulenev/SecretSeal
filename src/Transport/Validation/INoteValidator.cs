namespace Transport.Validation;

/// <summary>
/// Provides validation logic for note input received from transport layer.
/// </summary>
public interface INoteValidator
{
    /// <summary>
    /// Validates and normalizes the provided note content.
    /// </summary>
    /// <param name="note">
    /// Raw note value received from the request.
    /// May be <c>null</c>.
    /// </param>
    /// <returns>
    /// A <see cref="NoteValidationResult"/> describing whether the note is valid
    /// and containing the normalized value when validation succeeds.
    /// </returns>
    NoteValidationResult Validate(string? note);
}
