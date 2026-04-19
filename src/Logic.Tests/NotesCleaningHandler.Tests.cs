using Abstractions;

using FluentAssertions;

using Logic.Configuration;

using Microsoft.Extensions.Options;

using Moq;

namespace Logic.Tests;

public sealed class NotesCleaningHandlerTests
{
    [Fact(DisplayName = "Constructor throws when executor is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenExecutorIsNullThrowsArgumentNullException()
    {
        // Arrange
        INotesCleaningExecutor executor = null!;
        var options = Options.Create(new NotesCleanerOptions
        {
            DaysToKeep = 1,
            CleanupInterval = TimeSpan.FromSeconds(1)
        });

        // Act
        Action act = () => _ = new NotesCleaningHandler(executor, options);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when options are null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenOptionsIsNullThrowsArgumentNullException()
    {
        // Arrange
        var executor = new Mock<INotesCleaningExecutor>(MockBehavior.Strict).Object;
        IOptions<NotesCleanerOptions> options = null!;

        // Act
        Action act = () => _ = new NotesCleaningHandler(executor, options);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when options value is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenOptionsValueIsNullThrowsArgumentException()
    {
        // Arrange
        var executor = new Mock<INotesCleaningExecutor>(MockBehavior.Strict).Object;
        var optionsMock = new Mock<IOptions<NotesCleanerOptions>>(MockBehavior.Strict);
        optionsMock.SetupGet(opt => opt.Value).Returns((NotesCleanerOptions)null!);

        // Act
        Action act = () => _ = new NotesCleaningHandler(executor, optionsMock.Object);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "RunAsync executes cleanup and respects cancellation")]
    [Trait("Category", "Unit")]
    public async Task RunAsyncExecutesCleanupAndRespectsCancellation()
    {
        // Arrange
        var options = Options.Create(new NotesCleanerOptions
        {
            DaysToKeep = 1,
            CleanupInterval = TimeSpan.FromMilliseconds(1)
        });

        using var cts = new CancellationTokenSource();
        var cleanupCalls = 0;

        var executorMock = new Mock<INotesCleaningExecutor>(MockBehavior.Strict);
        executorMock
            .Setup(c => c.ExecuteOnceAsync(cts.Token))
            .Callback(() =>
            {
                cleanupCalls++;
                cts.Cancel();
            })
            .Returns(Task.CompletedTask);
        var handler = new NotesCleaningHandler(executorMock.Object, options);

        // Act
        Func<Task> act = () => handler.RunAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        cleanupCalls.Should().Be(1);
        executorMock.VerifyAll();
    }

    [Fact(DisplayName = "RunAsync stops before first execution when token is already cancelled")]
    [Trait("Category", "Unit")]
    public async Task RunAsyncWhenTokenIsAlreadyCancelledDoesNotExecuteCleanup()
    {
        // Arrange
        var options = Options.Create(new NotesCleanerOptions
        {
            DaysToKeep = 1,
            CleanupInterval = TimeSpan.FromMilliseconds(1)
        });
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var executorMock = new Mock<INotesCleaningExecutor>(MockBehavior.Strict);
        var handler = new NotesCleaningHandler(executorMock.Object, options);

        // Act
        Func<Task> act = () => handler.RunAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        executorMock.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "RunAsync cancels during delay after cleanup pass completes")]
    [Trait("Category", "Unit")]
    public async Task RunAsyncWhenTokenIsCancelledDuringDelayThrowsOperationCanceledException()
    {
        // Arrange
        var options = Options.Create(new NotesCleanerOptions
        {
            DaysToKeep = 1,
            CleanupInterval = TimeSpan.FromSeconds(30)
        });
        using var cts = new CancellationTokenSource();
        var cleanupCompleted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var cleanupCalls = 0;

        var executorMock = new Mock<INotesCleaningExecutor>(MockBehavior.Strict);
        executorMock
            .Setup(executor => executor.ExecuteOnceAsync(cts.Token))
            .Callback(() =>
            {
                cleanupCalls++;
                cleanupCompleted.TrySetResult(true);
            })
            .Returns(Task.CompletedTask);

        var handler = new NotesCleaningHandler(executorMock.Object, options);

        // Act
        var runTask = handler.RunAsync(cts.Token);
        await cleanupCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        await cts.CancelAsync();
        Func<Task> act = async () => await runTask;

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        cleanupCalls.Should().Be(1);
        executorMock.VerifyAll();
    }
}
