using Microsoft.Extensions.Options;

using Transport.Configuration;

namespace Transport.Validation;

/// <summary>
/// Default implementation of <see cref="INoteValidator"/>.
/// </summary>
public sealed class NoteValidator : INoteValidator
{
    private readonly NoteValidationOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoteValidator"/> class.
    /// </summary>
    /// <param name="options">
    /// Validation configuration containing note length limits.
    /// </param>
    public NoteValidator(IOptions<NoteValidationOptions> options)
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

#pragma warning disable IDE0046 // Convert to conditional expression
        if (max is not null && normalized.Length > max.Value)
        {
            return NoteValidationResult.Fail(
                $"This note is too large.");
        }
#pragma warning restore IDE0046 // Convert to conditional expression

        return NoteValidationResult.Success(normalized);
    }
}
