using Abstractions;

using FluentAssertions;

using Models;

using Moq;

namespace Logic.Tests;

public sealed class StorageNotesHandlerTests
{
    [Fact(DisplayName = "Constructor throws when unit of work is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenUnitOfWorkIsNullThrowsArgumentNullException()
    {
        // Arrange
        IUnitOfWork unitOfWork = null!;

        // Act
        Action act = () => _ = new StorageNotesHandler(unitOfWork);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "AddNoteAsync throws when note is null")]
    [Trait("Category", "Unit")]
    public async Task AddNoteAsyncWhenNoteIsNullThrowsArgumentNullException()
    {
        // Arrange
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict).Object;
        var handler = new StorageNotesHandler(unitOfWork);
        Note note = null!;
        var cancellationToken = new CancellationToken();

        // Act
        Func<Task> act = () => handler.AddNoteAsync(note, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact(DisplayName = "AddNoteAsync adds note and saves changes")]
    [Trait("Category", "Unit")]
    public async Task AddNoteAsyncAddsNoteAndSavesChanges()
    {
        // Arrange
        var note = new Note(new NoteId(Guid.NewGuid()), "content");
        var cancellationToken = new CancellationToken();
        var repoMock = new Mock<IRepository<Note, NoteId>>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var handler = new StorageNotesHandler(unitOfWorkMock.Object);

        var uowCount = 0;
        var repoCount = 0;

        unitOfWorkMock
            .SetupGet(work => work.Notes)
            .Returns(repoMock.Object);
        repoMock
            .Setup(repo => repo.AddAsync(note, cancellationToken))
            .Callback(() => repoCount++)
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => uowCount++)
            .Returns(Task.CompletedTask);

        // Act
        await handler.AddNoteAsync(note, cancellationToken);

        // Assert
        repoMock.VerifyAll();
        unitOfWorkMock.VerifyAll();
        uowCount.Should().Be(1);
        repoCount.Should().Be(1);
    }

    [Fact(DisplayName = "GetNotesCountAsync returns repository count")]
    [Trait("Category", "Unit")]
    public async Task GetNotesCountAsyncReturnsRepositoryCount()
    {
        // Arrange
        const long expected = 3;
        var cancellationToken = new CancellationToken();
        var repoMock = new Mock<IRepository<Note, NoteId>>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var handler = new StorageNotesHandler(unitOfWorkMock.Object);

        unitOfWorkMock
            .SetupGet(work => work.Notes)
            .Returns(repoMock.Object);
        repoMock
            .Setup(repo => repo.CountAsync(cancellationToken))
            .ReturnsAsync(expected);

        // Act
        var result = await handler.GetNotesCountAsync(cancellationToken);

        // Assert
        result.Should().Be(expected);
        repoMock.VerifyAll();
        unitOfWorkMock.VerifyAll();
    }

    [Fact(DisplayName = "TakeNoteAsync throws when note id is null")]
    [Trait("Category", "Unit")]
    public async Task TakeNoteAsyncWhenNoteIdIsNullThrowsArgumentNullException()
    {
        // Arrange
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict).Object;
        var handler = new StorageNotesHandler(unitOfWork);
        NoteId noteId = null!;
        var cancellationToken = new CancellationToken();

        // Act
        Func<Task> act = () => handler.TakeNoteAsync(noteId, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact(DisplayName = "TakeNoteAsync returns null when note does not exist")]
    [Trait("Category", "Unit")]
    public async Task TakeNoteAsyncReturnsNullWhenNoteDoesNotExist()
    {
        // Arrange
        var noteId = new NoteId(Guid.NewGuid());
        var cancellationToken = new CancellationToken();
        var repoMock = new Mock<IRepository<Note, NoteId>>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var handler = new StorageNotesHandler(unitOfWorkMock.Object);

        var uowCount = 0;

        unitOfWorkMock
            .SetupGet(work => work.Notes)
            .Returns(repoMock.Object);
        unitOfWorkMock
           .Setup(work => work.SaveChangesAsync(cancellationToken))
           .Callback(() => uowCount++)
           .Returns(Task.CompletedTask);
        repoMock
            .Setup(repo => repo.ConsumeAsync(noteId, cancellationToken))
            .ReturnsAsync((Note?)null);

        // Act
        var result = await handler.TakeNoteAsync(noteId, cancellationToken);

        // Assert
        result.Should().BeNull();
        repoMock.VerifyAll();
        unitOfWorkMock.VerifyAll();
        uowCount.Should().Be(1);
    }

    [Fact(DisplayName = "TakeNoteAsync removes note and saves changes")]
    [Trait("Category", "Unit")]
    public async Task TakeNoteAsyncRemovesNoteAndSavesChanges()
    {
        // Arrange
        var noteId = new NoteId(Guid.NewGuid());
        var note = new Note(noteId, "content");
        var cancellationToken = new CancellationToken();
        var repoMock = new Mock<IRepository<Note, NoteId>>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var handler = new StorageNotesHandler(unitOfWorkMock.Object);

        var removeCount = 0;
        var uowCount = 0;

        unitOfWorkMock
            .SetupGet(work => work.Notes)
            .Returns(repoMock.Object);
        repoMock
            .Setup(repo => repo.ConsumeAsync(noteId, cancellationToken))
            .Callback(() => removeCount++)
            .Returns(Task.FromResult<Note?>(note));
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => uowCount++)
            .Returns(Task.CompletedTask);

        // Act
        var result = await handler.TakeNoteAsync(noteId, cancellationToken);

        // Assert
        result.Should().Be(note);
        repoMock.VerifyAll();
        unitOfWorkMock.VerifyAll();
        removeCount.Should().Be(1);
        uowCount.Should().Be(1);
    }

    [Fact(DisplayName = "TakeNoteAsync does not remove note due race condition (note does not exists on a moment of remove operation")]
    [Trait("Category", "Unit")]
    public async Task TakeNoteAsyncDoesNotRemovesNoteThatJustWasDeleted()
    {
        // Arrange
        var noteId = new NoteId(Guid.NewGuid());
        var note = new Note(noteId, "content");
        var cancellationToken = new CancellationToken();
        var repoMock = new Mock<IRepository<Note, NoteId>>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var handler = new StorageNotesHandler(unitOfWorkMock.Object);

        var removeCount = 0;
        var uowCount = 0;

        unitOfWorkMock
            .SetupGet(work => work.Notes)
            .Returns(repoMock.Object);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => uowCount++)
            .Returns(Task.CompletedTask);
        repoMock
            .Setup(repo => repo.ConsumeAsync(noteId, cancellationToken))
            .Callback(() => removeCount++)
            .Returns(Task.FromResult<Note?>(note));

        // Act
        var result = await handler.TakeNoteAsync(noteId, cancellationToken);

        // Assert
        result.Should().Be(note);
        repoMock.VerifyAll();
        unitOfWorkMock.VerifyAll();
        removeCount.Should().Be(1);
        uowCount.Should().Be(1);
    }
}
