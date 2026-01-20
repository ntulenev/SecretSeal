namespace SecretSeal.Validation;

/// <summary>
/// Represents the result of note validation.
/// </summary>
/// <param name="IsValid">
/// Indicates whether the validation was successful.
/// </param>
/// <param name="Error">
/// Validation error message when validation fails; otherwise <c>null</c>.
/// </param>
/// <param name="NormalizedNote">
/// Trimmed and normalized note value when validation succeeds; otherwise <c>null</c>.
/// </param>
internal sealed record NoteValidationResult(
    bool IsValid,
    string? Error,
    string? NormalizedNote)
{
    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <param name="note">
    /// Normalized note value.
    /// </param>
    /// <returns>
    /// A successful <see cref="NoteValidationResult"/>.
    /// </returns>
    public static NoteValidationResult Success(string note)
        => new(true, null, note);

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="error">
    /// Human-readable validation error message.
    /// </param>
    /// <returns>
    /// A failed <see cref="NoteValidationResult"/>.
    /// </returns>
    public static NoteValidationResult Fail(string error)
        => new(false, error, null);
}
