using Abstractions;

using FluentAssertions;

using Models;

using SecretSeal.UseCases;

using Transport;
using Transport.Validation;

namespace SecretSeal.Tests;

public sealed class CreateNoteUseCaseTests
{
    [Fact(DisplayName = "Constructor throws when validator is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValidatorIsNullThrowsArgumentNullException()
    {
        // Arrange
        INoteValidator validator = null!;
        var handler = new RecordingNotesHandler();

        // Act
        Action act = () => _ = new CreateNoteUseCase(validator, handler);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when handler is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenHandlerIsNullThrowsArgumentNullException()
    {
        // Arrange
        var validator = new StubNoteValidator(_ => NoteValidationResult.Success("normalized"));
        INotesHandler handler = null!;

        // Act
        Action act = () => _ = new CreateNoteUseCase(validator, handler);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "ExecuteAsync throws when request is null")]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsyncWhenRequestIsNullThrowsArgumentNullException()
    {
        // Arrange
        var useCase = new CreateNoteUseCase(
            new StubNoteValidator(_ => NoteValidationResult.Success("normalized")),
            new RecordingNotesHandler());
        CreateNoteRequest request = null!;

        // Act
        Func<Task> act = () => useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact(DisplayName = "ExecuteAsync returns validation error and does not persist note")]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsyncWhenValidationFailsReturnsError()
    {
        // Arrange
        var handler = new RecordingNotesHandler();
        var useCase = new CreateNoteUseCase(
            new StubNoteValidator(_ => NoteValidationResult.Fail("invalid")),
            handler);

        // Act
        var result = await useCase.ExecuteAsync(new CreateNoteRequest("bad"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("invalid");
        result.Response.Should().BeNull();
        handler.AddedNote.Should().BeNull();
    }

    [Fact(DisplayName = "ExecuteAsync creates note from normalized content and returns response")]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsyncWhenValidationSucceedsPersistsNote()
    {
        // Arrange
        var handler = new RecordingNotesHandler();
        var useCase = new CreateNoteUseCase(
            new StubNoteValidator(_ => NoteValidationResult.Success("trimmed")),
            handler);

        // Act
        var result = await useCase.ExecuteAsync(new CreateNoteRequest("  trimmed  "), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        result.Response.Should().NotBeNull();
        handler.AddedNote.Should().NotBeNull();
        handler.AddedNote!.Content.Should().Be("trimmed");
        result.Response!.Id.Value.Should().Be(handler.AddedNote.Id.Value);
    }

    private sealed class StubNoteValidator(Func<string?, NoteValidationResult> validate) : INoteValidator
    {
        public NoteValidationResult Validate(string? note) => validate(note);
    }

    private sealed class RecordingNotesHandler : INotesHandler
    {
        public Note? AddedNote { get; private set; }

        public Task AddNoteAsync(Note note, CancellationToken cancellationToken)
        {
            AddedNote = note;
            return Task.CompletedTask;
        }

        public Task<long> GetNotesCountAsync(CancellationToken cancellationToken) => Task.FromResult(0L);

        public Task<Note?> TakeNoteAsync(NoteId noteId, CancellationToken cancellationToken) =>
            Task.FromResult<Note?>(null);
    }
}
