using Abstractions;

using Models;

using Transport;
using Transport.Validation;

namespace SecretSeal.UseCases;

/// <summary>
/// Creates a note from an incoming transport request.
/// </summary>
internal sealed class CreateNoteUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateNoteUseCase"/> class.
    /// </summary>
    /// <param name="validator">The validator used to validate and normalize incoming note content.</param>
    /// <param name="handler">The handler used to persist the created note.</param>
    public CreateNoteUseCase(INoteValidator validator, INotesHandler handler)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(handler);
        _validator = validator;
        _handler = handler;
    }

    /// <summary>
    /// Validates the request, creates a note, and stores it.
    /// </summary>
    /// <param name="request">The incoming create-note request.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The result of the create-note operation.</returns>
    public async Task<CreateNoteUseCaseResult> ExecuteAsync(
        CreateNoteRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = _validator.Validate(request.Note);
        if (!validationResult.IsValid)
        {
            return new CreateNoteUseCaseResult(null, validationResult.Error);
        }

        var note = Note.Create(validationResult.NormalizedNote!);
        await _handler.AddNoteAsync(note, cancellationToken).ConfigureAwait(false);

        return new CreateNoteUseCaseResult(new CreateNoteResponse(note.Id.Value), null);
    }

    private readonly INotesHandler _handler;
    private readonly INoteValidator _validator;
}

/// <summary>
/// Represents the outcome of the create-note use case.
/// </summary>
/// <param name="Response">The successful create-note response, or <see langword="null"/> when the request failed.</param>
/// <param name="Error">The validation or execution error, or <see langword="null"/> when the request succeeded.</param>
internal sealed record CreateNoteUseCaseResult(CreateNoteResponse? Response, string? Error)
{
    /// <summary>
    /// Gets a value indicating whether the use case completed successfully.
    /// </summary>
    public bool IsSuccess => Error is null;
}
