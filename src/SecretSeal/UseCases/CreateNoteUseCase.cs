using Abstractions;

using Models;

using Transport;
using Transport.Validation;

namespace SecretSeal.UseCases;

internal sealed class CreateNoteUseCase
{
    public CreateNoteUseCase(INoteValidator validator, INotesHandler handler)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(handler);
        _validator = validator;
        _handler = handler;
    }

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

internal sealed record CreateNoteUseCaseResult(CreateNoteResponse? Response, string? Error)
{
    public bool IsSuccess => Error is null;
}
