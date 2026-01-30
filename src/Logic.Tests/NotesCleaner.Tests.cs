using Abstractions;

using FluentAssertions;

using Logic.Configuration;

using Microsoft.Extensions.Options;

using Models;

using Moq;

namespace Logic.Tests;

public sealed class NotesCleanerTests
{
    [Fact(DisplayName = "Constructor throws when unit of work is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenUnitOfWorkIsNullThrowsArgumentNullException()
    {
        // Arrange
        IUnitOfWork unitOfWork = null!;
        var options = Options.Create(new NotesCleanerOptions { DaysToKeep = 1 });

        // Act
        Action act = () => _ = new NotesCleaner(unitOfWork, options);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when options are null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenOptionsIsNullThrowsArgumentNullException()
    {
        // Arrange
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict).Object;
        IOptions<NotesCleanerOptions> options = null!;

        // Act
        Action act = () => _ = new NotesCleaner(unitOfWork, options);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when options value is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenOptionsValueIsNullThrowsArgumentException()
    {
        // Arrange
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict).Object;
        var optionsMock = new Mock<IOptions<NotesCleanerOptions>>(MockBehavior.Strict);
        optionsMock.SetupGet(opt => opt.Value).Returns((NotesCleanerOptions)null!);

        // Act
        Action act = () => _ = new NotesCleaner(unitOfWork, optionsMock.Object);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "RemoveObsoleteNotesAsync removes notes and saves changes")]
    [Trait("Category", "Unit")]
    public async Task RemoveObsoleteNotesAsyncRemovesNotesAndSavesChanges()
    {
        // Arrange
        const int daysToKeep = 7;
        var cancellationToken = new CancellationToken();
        var options = Options.Create(new NotesCleanerOptions { DaysToKeep = daysToKeep });
        var repoMock = new Mock<IRepository<Note, NoteId>>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var cleaner = new NotesCleaner(unitOfWorkMock.Object, options);

        var before = DateTimeOffset.UtcNow;
        var removeCalls = 0;
        var saveCalls = 0;

        unitOfWorkMock
            .SetupGet(work => work.Notes)
            .Returns(repoMock.Object);
        repoMock
            .Setup(repo => repo.RemoveObsoleteNotesAsync(
                It.Is<DateTimeOffset>(cutoff =>
                    cutoff >= before.AddDays(-daysToKeep) &&
                    cutoff <= DateTimeOffset.UtcNow.AddDays(-daysToKeep)),
                cancellationToken))
            .Callback(() => removeCalls++)
            .ReturnsAsync(3);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        // Act
        await cleaner.RemoveObsoleteNotesAsync(cancellationToken);

        // Assert
        repoMock.VerifyAll();
        unitOfWorkMock.VerifyAll();
        removeCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }
}
