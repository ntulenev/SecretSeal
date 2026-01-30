using Abstractions;

using FluentAssertions;

using Logic.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Moq;

namespace Logic.Tests;

public sealed class NotesCleaningHandlerTests
{
    [Fact(DisplayName = "Constructor throws when scope factory is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenScopeFactoryIsNullThrowsArgumentNullException()
    {
        // Arrange
        IServiceScopeFactory scopeFactory = null!;
        var options = Options.Create(new NotesCleanerOptions
        {
            DaysToKeep = 1,
            CleanupInterval = TimeSpan.FromSeconds(1)
        });

        // Act
        Action act = () => _ = new NotesCleaningHandler(scopeFactory, options);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when options are null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenOptionsIsNullThrowsArgumentNullException()
    {
        // Arrange
        var scopeFactory = new Mock<IServiceScopeFactory>(MockBehavior.Strict).Object;
        IOptions<NotesCleanerOptions> options = null!;

        // Act
        Action act = () => _ = new NotesCleaningHandler(scopeFactory, options);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when options value is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenOptionsValueIsNullThrowsArgumentException()
    {
        // Arrange
        var scopeFactory = new Mock<IServiceScopeFactory>(MockBehavior.Strict).Object;
        var optionsMock = new Mock<IOptions<NotesCleanerOptions>>(MockBehavior.Strict);
        optionsMock.SetupGet(opt => opt.Value).Returns((NotesCleanerOptions)null!);

        // Act
        Action act = () => _ = new NotesCleaningHandler(scopeFactory, optionsMock.Object);

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
        var scopeCalls = 0;

        var cleanerMock = new Mock<INotesCleaner>(MockBehavior.Strict);
        cleanerMock
            .Setup(c => c.RemoveObsoleteNotesAsync(cts.Token))
            .Callback(() =>
            {
                cleanupCalls++;
                cts.Cancel();
            })
            .Returns(Task.CompletedTask);

        var providerMock = new Mock<IServiceProvider>(MockBehavior.Strict);
        providerMock
            .Setup(p => p.GetService(typeof(INotesCleaner)))
            .Returns(cleanerMock.Object);

        var scopeMock = new Mock<IServiceScope>(MockBehavior.Strict);
        scopeMock.SetupGet(s => s.ServiceProvider).Returns(providerMock.Object);
        scopeMock
            .Setup(s => s.Dispose())
            .Callback(() => scopeCalls++);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>(MockBehavior.Strict);
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        var handler = new NotesCleaningHandler(scopeFactoryMock.Object, options);

        // Act
        Func<Task> act = () => handler.RunAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        cleanupCalls.Should().Be(1);
        scopeCalls.Should().Be(1);
    }
}
