using FluentAssertions;

using Logic.Configuration;

using Microsoft.Extensions.Options;

using SecretSeal.Configuration;
using SecretSeal.UseCases;

namespace SecretSeal.Tests;

public sealed class GetRetentionPolicyUseCaseTests
{
    [Fact(DisplayName = "Constructor throws when cleaner options are null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCleanerOptionsIsNullThrowsArgumentNullException()
    {
        // Arrange
        IOptions<NotesCleanerOptions> cleanerOptions = null!;
        var storageOptions = Options.Create(new StorageOptions { Mode = StorageMode.InMemory });

        // Act
        Action act = () => _ = new GetRetentionPolicyUseCase(cleanerOptions, storageOptions);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when storage options are null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenStorageOptionsIsNullThrowsArgumentNullException()
    {
        // Arrange
        var cleanerOptions = Options.Create(new NotesCleanerOptions
        {
            DaysToKeep = 7,
            CleanupInterval = TimeSpan.FromHours(1)
        });
        IOptions<StorageOptions> storageOptions = null!;

        // Act
        Action act = () => _ = new GetRetentionPolicyUseCase(cleanerOptions, storageOptions);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Execute returns unlimited retention in memory mode")]
    [Trait("Category", "Unit")]
    public void ExecuteWhenStorageModeIsInMemoryReturnsMinusOne()
    {
        // Arrange
        var useCase = new GetRetentionPolicyUseCase(
            Options.Create(new NotesCleanerOptions
            {
                DaysToKeep = 14,
                CleanupInterval = TimeSpan.FromHours(1)
            }),
            Options.Create(new StorageOptions { Mode = StorageMode.InMemory }));

        // Act
        var result = useCase.Execute();

        // Assert
        result.DaysToKeep.Should().Be(-1);
    }

    [Fact(DisplayName = "Execute returns configured retention in database mode")]
    [Trait("Category", "Unit")]
    public void ExecuteWhenStorageModeIsDatabaseReturnsConfiguredDays()
    {
        // Arrange
        var useCase = new GetRetentionPolicyUseCase(
            Options.Create(new NotesCleanerOptions
            {
                DaysToKeep = 14,
                CleanupInterval = TimeSpan.FromHours(1)
            }),
            Options.Create(new StorageOptions { Mode = StorageMode.Database }));

        // Act
        var result = useCase.Execute();

        // Assert
        result.DaysToKeep.Should().Be(14);
    }
}
