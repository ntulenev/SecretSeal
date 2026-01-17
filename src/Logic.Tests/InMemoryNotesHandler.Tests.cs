using FluentAssertions;

using Models;

namespace Logic.Tests;

public sealed class InMemoryNotesHandlerTests
{
    [Fact(DisplayName = "AddNoteAsync throws when note is null")]
    [Trait("Category", "Unit")]
    public async Task AddNoteAsyncWhenNoteIsNullThrowsArgumentNullException()
    {
        //Arrage
        var handler = new InMemoryNotesHandler();
        Note note = null!;
        var cancellationToken = new CancellationToken();

        //Act
        Func<Task> act = () => handler.AddNoteAsync(note, cancellationToken);

        //Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact(DisplayName = "AddNoteAsync adds note and GetNotesCountAsync returns count")]
    [Trait("Category", "Unit")]
    public async Task AddNoteAsyncAddsNoteAndCountReturnsOne()
    {
        //Arrage
        var handler = new InMemoryNotesHandler();
        var noteId = new NoteId(Guid.NewGuid());
        var note = new Note(noteId, "content");
        var cancellationToken = new CancellationToken();

        //Act
        await handler.AddNoteAsync(note, cancellationToken);
        var count = await handler.GetNotesCountAsync(cancellationToken);

        //Assert
        count.Should().Be(1);
    }

    //TODO: With the fact that the keys are GUIDs, this scenario is impossible, but it’s probably worth adding a fallback
    //so that if, for some reason, the GUIDs do match, they are regenerated.
    [Fact(DisplayName = "AddNoteAsync replaces existing note with same id")]
    [Trait("Category", "Unit")]
    public async Task AddNoteAsyncReplacesNoteWithSameId()
    {
        //Arrage
        var handler = new InMemoryNotesHandler();
        var noteId = new NoteId(Guid.NewGuid());
        var first = new Note(noteId, "first");
        var second = new Note(noteId, "second");
        var cancellationToken = new CancellationToken();

        //Act
        await handler.AddNoteAsync(first, cancellationToken);
        await handler.AddNoteAsync(second, cancellationToken);
        var result = await handler.TakeNoteAsync(noteId, cancellationToken);

        //Assert
        result.Should().NotBeNull();
        result!.Content.Should().Be("second");
    }

    [Fact(DisplayName = "TakeNoteAsync throws when note id is null")]
    [Trait("Category", "Unit")]
    public async Task TakeNoteAsyncWhenNoteIdIsNullThrowsArgumentNullException()
    {
        //Arrage
        var handler = new InMemoryNotesHandler();
        NoteId noteId = null!;
        var cancellationToken = new CancellationToken();

        //Act
        Func<Task> act = () => handler.TakeNoteAsync(noteId, cancellationToken);

        //Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact(DisplayName = "TakeNoteAsync returns null when note does not exist")]
    [Trait("Category", "Unit")]
    public async Task TakeNoteAsyncReturnsNullWhenNoteDoesNotExist()
    {
        //Arrage
        var handler = new InMemoryNotesHandler();
        var noteId = new NoteId(Guid.NewGuid());
        var cancellationToken = new CancellationToken();

        //Act
        var result = await handler.TakeNoteAsync(noteId, cancellationToken);

        //Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "TakeNoteAsync removes and returns the stored note")]
    [Trait("Category", "Unit")]
    public async Task TakeNoteAsyncRemovesAndReturnsStoredNote()
    {
        //Arrage
        var handler = new InMemoryNotesHandler();
        var noteId = new NoteId(Guid.NewGuid());
        var note = new Note(noteId, "content");
        var cancellationToken = new CancellationToken();

        await handler.AddNoteAsync(note, cancellationToken);

        //Act
        var result = await handler.TakeNoteAsync(noteId, cancellationToken);
        var count = await handler.GetNotesCountAsync(cancellationToken);

        //Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(noteId);
        result.Content.Should().Be("content");
        count.Should().Be(0);
    }
}
