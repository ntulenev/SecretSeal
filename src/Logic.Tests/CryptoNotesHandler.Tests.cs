using Abstractions;

using FluentAssertions;

using Models;

using Moq;

namespace Logic.Tests;

public sealed class CryptoNotesHandlerTests
{
    [Fact(DisplayName = "Constructor throws when notes handler is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenNotesHandlerIsNullThrowsArgumentNullException()
    {
        // Arrange
        INotesHandler notesHandler = null!;
        var cryptoHelper = new Mock<ICryptoHelper>(MockBehavior.Strict).Object;

        // Act
        Action act = () => _ = new CryptoNotesHandler(notesHandler, cryptoHelper);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when crypto helper is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCryptoHelperIsNullThrowsArgumentNullException()
    {
        // Arrange
        var notesHandler = new Mock<INotesHandler>(MockBehavior.Strict).Object;
        ICryptoHelper cryptoHelper = null!;

        // Act
        Action act = () => _ = new CryptoNotesHandler(notesHandler, cryptoHelper);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "AddNoteAsync throws when note is null")]
    [Trait("Category", "Unit")]
    public async Task AddNoteAsyncWhenNoteIsNullThrowsArgumentNullException()
    {
        // Arrange
        var notesHandler = new Mock<INotesHandler>(MockBehavior.Strict).Object;
        var cryptoHelper = new Mock<ICryptoHelper>(MockBehavior.Strict).Object;
        var handler = new CryptoNotesHandler(notesHandler, cryptoHelper);
        Note note = null!;
        var cancellationToken = new CancellationToken();

        // Act
        Func<Task> act = () => handler.AddNoteAsync(note, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact(DisplayName = "AddNoteAsync encrypts note content and forwards to inner handler")]
    [Trait("Category", "Unit")]
    public async Task AddNoteAsyncEncryptsContentAndForwardsToInnerHandler()
    {
        // Arrange
        var noteId = new NoteId(Guid.NewGuid());
        var note = new Note(noteId, "plain");
        var encrypted = "encrypted";
        var cancellationToken = new CancellationToken();
        var notesHandlerMock = new Mock<INotesHandler>(MockBehavior.Strict);
        var cryptoHelperMock = new Mock<ICryptoHelper>(MockBehavior.Strict);
        var handler = new CryptoNotesHandler(notesHandlerMock.Object, cryptoHelperMock.Object);
        var encryptCalls = 0;
        var addNoteCalls = 0;
        cryptoHelperMock
            .Setup(helper => helper.Encrypt(note.Content))
            .Callback(() => encryptCalls++)
            .Returns(encrypted);
        notesHandlerMock
            .Setup(inner => inner.AddNoteAsync(
                It.Is<Note>(n => n.Id == noteId && n.Content == encrypted),
                cancellationToken))
            .Callback(() => addNoteCalls++)
            .Returns(Task.CompletedTask);

        // Act
        await handler.AddNoteAsync(note, cancellationToken);

        // Assert
        cryptoHelperMock.VerifyAll();
        notesHandlerMock.VerifyAll();
        encryptCalls.Should().Be(1);
        addNoteCalls.Should().Be(1);
    }

    [Fact(DisplayName = "GetNotesCountAsync returns the inner handler count")]
    [Trait("Category", "Unit")]
    public async Task GetNotesCountAsyncReturnsInnerHandlerCount()
    {
        // Arrange
        const long expected = 12;
        var cancellationToken = new CancellationToken();
        var notesHandlerMock = new Mock<INotesHandler>(MockBehavior.Strict);
        var cryptoHelperMock = new Mock<ICryptoHelper>(MockBehavior.Strict);
        var handler = new CryptoNotesHandler(notesHandlerMock.Object, cryptoHelperMock.Object);
        var getNotesCountCalls = 0;

        notesHandlerMock
            .Setup(inner => inner.GetNotesCountAsync(cancellationToken))
            .Callback(() => getNotesCountCalls++)
            .ReturnsAsync(expected);

        // Act
        var result = await handler.GetNotesCountAsync(cancellationToken);

        // Assert
        result.Should().Be(expected);
        notesHandlerMock.VerifyAll();
        getNotesCountCalls.Should().Be(1);
    }

    [Fact(DisplayName = "TakeNoteAsync throws when note id is null")]
    [Trait("Category", "Unit")]
    public async Task TakeNoteAsyncWhenNoteIdIsNullThrowsArgumentNullException()
    {
        // Arrange
        var notesHandler = new Mock<INotesHandler>(MockBehavior.Strict).Object;
        var cryptoHelper = new Mock<ICryptoHelper>(MockBehavior.Strict).Object;
        var handler = new CryptoNotesHandler(notesHandler, cryptoHelper);
        NoteId noteId = null!;
        var cancellationToken = new CancellationToken();

        // Act
        Func<Task> act = () => handler.TakeNoteAsync(noteId, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact(DisplayName = "TakeNoteAsync returns null when inner handler returns null")]
    [Trait("Category", "Unit")]
    public async Task TakeNoteAsyncReturnsNullWhenInnerHandlerReturnsNull()
    {
        // Arrange
        var noteId = new NoteId(Guid.NewGuid());
        var cancellationToken = new CancellationToken();
        var notesHandlerMock = new Mock<INotesHandler>(MockBehavior.Strict);
        var cryptoHelperMock = new Mock<ICryptoHelper>(MockBehavior.Strict);
        var handler = new CryptoNotesHandler(notesHandlerMock.Object, cryptoHelperMock.Object);
        var takeNoteCalls = 0;

        notesHandlerMock
            .Setup(inner => inner.TakeNoteAsync(noteId, cancellationToken))
            .Callback(() => takeNoteCalls++)
            .ReturnsAsync((Note?)null);

        // Act
        var result = await handler.TakeNoteAsync(noteId, cancellationToken);

        // Assert
        result.Should().BeNull();
        notesHandlerMock.VerifyAll();
        takeNoteCalls.Should().Be(1);
    }

    [Fact(DisplayName = "TakeNoteAsync decrypts content and returns a new note")]
    [Trait("Category", "Unit")]
    public async Task TakeNoteAsyncDecryptsContentAndReturnsNote()
    {
        // Arrange
        var noteId = new NoteId(Guid.NewGuid());
        var encrypted = "encrypted";
        var decrypted = "plain";
        var storedNote = new Note(noteId, encrypted);
        var cancellationToken = new CancellationToken();
        var notesHandlerMock = new Mock<INotesHandler>(MockBehavior.Strict);
        var cryptoHelperMock = new Mock<ICryptoHelper>(MockBehavior.Strict);
        var handler = new CryptoNotesHandler(notesHandlerMock.Object, cryptoHelperMock.Object);
        var takeNoteCalls = 0;
        var decryptCalls = 0;

        notesHandlerMock
            .Setup(inner => inner.TakeNoteAsync(noteId, cancellationToken))
            .Callback(() => takeNoteCalls++)
            .ReturnsAsync(storedNote);
        cryptoHelperMock
            .Setup(helper => helper.Decrypt(encrypted))
            .Callback(() => decryptCalls++)
            .Returns(decrypted);

        // Act
        var result = await handler.TakeNoteAsync(noteId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(noteId);
        result.Content.Should().Be(decrypted);
        notesHandlerMock.VerifyAll();
        cryptoHelperMock.VerifyAll();
        takeNoteCalls.Should().Be(1);
        decryptCalls.Should().Be(1);
    }
}
