using Abstractions;

using FluentAssertions;

using SecretSeal.Services;

namespace SecretSeal.Tests;

public sealed class NotesCleanerServiceTests
{
    [Fact(DisplayName = "Constructor throws when cleaning handler is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCleaningHandlerIsNullThrowsArgumentNullException()
    {
        // Arrange
        INotesCleaningHandler cleaningHandler = null!;

        // Act
        Action act = () => _ = new NotesCleanerService(cleaningHandler);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "StartAsync runs the cleaning handler")]
    [Trait("Category", "Unit")]
    public async Task StartAsyncRunsCleaningHandler()
    {
        // Arrange
        var handler = new RecordingCleaningHandler();
        using var service = new NotesCleanerService(handler);

        // Act
        await service.StartAsync(CancellationToken.None);
        await handler.Task.WaitAsync(TimeSpan.FromSeconds(1));
        await service.StopAsync(CancellationToken.None);

        // Assert
        handler.Calls.Should().Be(1);
    }

    private sealed class RecordingCleaningHandler : INotesCleaningHandler
    {
        private readonly TaskCompletionSource<bool> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int Calls { get; private set; }

        public Task RunAsync(CancellationToken cancellationToken)
        {
            Calls++;
            _tcs.TrySetResult(true);
            return Task.CompletedTask;
        }

        public Task Task => _tcs.Task;
    }
}
