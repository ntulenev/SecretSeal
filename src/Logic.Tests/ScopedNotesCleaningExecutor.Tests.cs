using Abstractions;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace Logic.Tests;

public sealed class ScopedNotesCleaningExecutorTests
{
    [Fact(DisplayName = "Constructor throws when scope factory is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenScopeFactoryIsNullThrowsArgumentNullException()
    {
        // Arrange
        IServiceScopeFactory scopeFactory = null!;

        // Act
        Action act = () => _ = new ScopedNotesCleaningExecutor(scopeFactory);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "ExecuteOnceAsync resolves cleaner in a scope")]
    [Trait("Category", "Unit")]
    public async Task ExecuteOnceAsyncResolvesCleanerInScope()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var cleanupCalls = 0;
        var scopeDisposeCalls = 0;

        var cleanerMock = new Mock<INotesCleaner>(MockBehavior.Strict);
        cleanerMock
            .Setup(c => c.RemoveObsoleteNotesAsync(cancellationToken))
            .Callback(() => cleanupCalls++)
            .Returns(Task.CompletedTask);

        var providerMock = new Mock<IServiceProvider>(MockBehavior.Strict);
        providerMock
            .Setup(p => p.GetService(typeof(INotesCleaner)))
            .Returns(cleanerMock.Object);

        var scopeMock = new Mock<IServiceScope>(MockBehavior.Strict);
        scopeMock.SetupGet(s => s.ServiceProvider).Returns(providerMock.Object);
        scopeMock
            .Setup(s => s.Dispose())
            .Callback(() => scopeDisposeCalls++);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>(MockBehavior.Strict);
        scopeFactoryMock
            .Setup(f => f.CreateScope())
            .Returns(scopeMock.Object);

        var executor = new ScopedNotesCleaningExecutor(scopeFactoryMock.Object);

        // Act
        await executor.ExecuteOnceAsync(cancellationToken);

        // Assert
        cleanerMock.VerifyAll();
        scopeFactoryMock.VerifyAll();
        cleanupCalls.Should().Be(1);
        scopeDisposeCalls.Should().Be(1);
    }
}
