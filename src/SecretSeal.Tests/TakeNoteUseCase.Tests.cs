using Abstractions;

using FluentAssertions;

using Models;

using SecretSeal.UseCases;

using Transport;

namespace SecretSeal.Tests;

public sealed class TakeNoteUseCaseTests
{
    [Fact(DisplayName = "Constructor throws when handler is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenHandlerIsNullThrowsArgumentNullException()
    {
        // Arrange
        INotesHandler handler = null!;

        // Act
        Action act = () => _ = new TakeNoteUseCase(handler);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "ExecuteAsync returns null when note does not exist")]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsyncWhenNoteDoesNotExistReturnsNull()
    {
        // Arrange
        var handler = new StubNotesHandler(null);
        var useCase = new TakeNoteUseCase(handler);
        var id = ShortGuid.FromGuid(Guid.NewGuid());

        // Act
        var result = await useCase.ExecuteAsync(id, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "ExecuteAsync maps existing note to transport response")]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsyncWhenNoteExistsReturnsResponse()
    {
        // Arrange
        var note = Note.Create("restored");
        var handler = new StubNotesHandler(note);
        var useCase = new TakeNoteUseCase(handler);
        var id = ShortGuid.FromGuid(note.Id.Value);

        // Act
        var result = await useCase.ExecuteAsync(id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Value.Should().Be(note.Id.Value);
        result.Note.Should().Be(note.Content);
    }

    private sealed class StubNotesHandler(Note? note) : INotesHandler
    {
        public Task AddNoteAsync(Note value, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<long> GetNotesCountAsync(CancellationToken cancellationToken) => Task.FromResult(0L);

        public Task<Note?> TakeNoteAsync(NoteId noteId, CancellationToken cancellationToken) =>
            Task.FromResult(note);
    }
}
