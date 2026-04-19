using Abstractions;

using Cryptography;
using Cryptography.Configuration;

using FluentAssertions;

using Logic;

using Microsoft.Extensions.Options;

using Models;

using SecretSeal.Configuration;
using SecretSeal.UseCases;

namespace SecretSeal.Tests;

public sealed class GetNoteStatsUseCaseTests
{
    [Fact(DisplayName = "Constructor throws when handler is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenHandlerIsNullThrowsArgumentNullException()
    {
        // Arrange
        INotesHandler handler = null!;
        var storageOptions = Options.Create(new StorageOptions { Mode = StorageMode.InMemory });

        // Act
        Action act = () => _ = new GetNoteStatsUseCase(handler, storageOptions);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when storage options are null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenStorageOptionsIsNullThrowsArgumentNullException()
    {
        // Arrange
        var handler = new StubNotesHandler(0);
        IOptions<StorageOptions> storageOptions = null!;

        // Act
        Action act = () => _ = new GetNoteStatsUseCase(handler, storageOptions);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "ExecuteAsync returns count and enabled flags for encrypted in-memory handler")]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsyncWhenHandlerIsEncryptedInMemoryReturnsFlags()
    {
        // Arrange
        var innerHandler = new StubNotesHandler(3);
        var handler = new CryptoNotesHandler(
            innerHandler,
            new CryptoHelper(Options.Create(new CryptoOptions
            {
                Key = "12345678901234567890123456789012"
            })));
        var useCase = new GetNoteStatsUseCase(
            handler,
            Options.Create(new StorageOptions { Mode = StorageMode.InMemory }));

        // Act
        var result = await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        result.NotesCount.Should().Be(3);
        result.EncryptionEnabled.Should().BeTrue();
        result.IsInMemory.Should().BeTrue();
    }

    [Fact(DisplayName = "ExecuteAsync returns count and disabled flags for non-encrypted database handler")]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsyncWhenHandlerIsNotEncryptedReturnsFlags()
    {
        // Arrange
        var handler = new StubNotesHandler(5);
        var useCase = new GetNoteStatsUseCase(
            handler,
            Options.Create(new StorageOptions { Mode = StorageMode.Database }));

        // Act
        var result = await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        result.NotesCount.Should().Be(5);
        result.EncryptionEnabled.Should().BeFalse();
        result.IsInMemory.Should().BeFalse();
    }

    private sealed class StubNotesHandler(long count) : INotesHandler
    {
        public Task AddNoteAsync(Note note, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<long> GetNotesCountAsync(CancellationToken cancellationToken) => Task.FromResult(count);

        public Task<Note?> TakeNoteAsync(NoteId noteId, CancellationToken cancellationToken) =>
            Task.FromResult<Note?>(null);
    }
}
