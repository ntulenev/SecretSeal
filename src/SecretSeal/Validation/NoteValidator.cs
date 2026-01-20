using Microsoft.Extensions.Options;

using SecretSeal.Configuration;

namespace SecretSeal.Validation;

/// <summary>
/// Default implementation of <see cref="INoteValidator"/>.
/// </summary>
internal sealed class NoteValidator : INoteValidator
{
    private readonly StorageOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoteValidator"/> class.
    /// </summary>
    /// <param name="options">
    /// Storage configuration containing note length limits.
    /// </param>
    public NoteValidator(IOptions<StorageOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
    }

    /// <inheritdoc />
    public NoteValidationResult Validate(string? note)
    {
        var normalized = note?.Trim();

        if (string.IsNullOrEmpty(normalized))
        {
            return NoteValidationResult.Fail(
                "Note must not be empty.");
        }

        var max = _options.MaxNoteLength;

        if (max is not null && normalized.Length > max.Value)
        {
            return NoteValidationResult.Fail(
                $"Note must not be longer than {max.Value} characters.");
        }

        return NoteValidationResult.Success(normalized);
    }
}
