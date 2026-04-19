using Abstractions;

using Cryptography.Configuration;

using FluentAssertions;

using Logic.Configuration;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using SecretSeal.Configuration;
using SecretSeal.Routing;
using SecretSeal.Services;
using SecretSeal.Startup;
using SecretSeal.UseCases;

using Storage;
using Storage.Repositories;

using Transport.Configuration;
using Transport.Serialization;
using Transport.Validation;

namespace SecretSeal.Tests;

public sealed class StartupCompositionTests
{
    [Fact(DisplayName = "AddSecretSealOptions throws when services are null")]
    [Trait("Category", "Unit")]
    public void AddSecretSealOptionsWhenServicesAreNullThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;
        var configuration = CreateConfiguration();

        // Act
        Action act = () => _ = ServiceCollectionOptionsExtensions.AddSecretSealOptions(services, configuration);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "AddSecretSealOptions throws when configuration is null")]
    [Trait("Category", "Unit")]
    public void AddSecretSealOptionsWhenConfigurationIsNullThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfiguration configuration = null!;

        // Act
        Action act = () => _ = services.AddSecretSealOptions(configuration);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "AddSecretSealOptions binds valid options")]
    [Trait("Category", "Unit")]
    public void AddSecretSealOptionsBindsConfiguredValues()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        services.AddSecretSealOptions(configuration);
        using var provider = services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<IOptions<StorageOptions>>().Value.Mode.Should().Be(StorageMode.InMemory);
        provider.GetRequiredService<IOptions<NoteValidationOptions>>().Value.MaxNoteLength.Should().Be(15);
        provider.GetRequiredService<IOptions<CryptoOptions>>().Value.Key.Should().Be("12345678901234567890123456789012");
        provider.GetRequiredService<IOptions<NotesCleanerOptions>>().Value.DaysToKeep.Should().Be(14);
    }

    [Fact(DisplayName = "AddSecretSealOptions rejects invalid max note length")]
    [Trait("Category", "Unit")]
    public void AddSecretSealOptionsWhenMaxNoteLengthIsInvalidThrowsOptionsValidationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(("Validation:MaxNoteLength", "0"));

        services.AddSecretSealOptions(configuration);
        using var provider = services.BuildServiceProvider();

        // Act
        Action act = () => _ = provider.GetRequiredService<IOptions<NoteValidationOptions>>().Value;

        // Assert
        act.Should().Throw<OptionsValidationException>();
    }

    [Fact(DisplayName = "AddSecretSealOptions rejects invalid cleanup interval")]
    [Trait("Category", "Unit")]
    public void AddSecretSealOptionsWhenCleanupIntervalIsInvalidThrowsOptionsValidationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(("NotesCleaner:CleanupInterval", "00:00:00"));

        services.AddSecretSealOptions(configuration);
        using var provider = services.BuildServiceProvider();

        // Act
        Action act = () => _ = provider.GetRequiredService<IOptions<NotesCleanerOptions>>().Value;

        // Assert
        act.Should().Throw<OptionsValidationException>();
    }

    [Fact(DisplayName = "AddSecretSealRouting throws when services are null")]
    [Trait("Category", "Unit")]
    public void AddSecretSealRoutingWhenServicesAreNullThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act
        Action act = () => _ = ServiceCollectionRoutingExtensions.AddSecretSealRouting(services);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "AddSecretSealRouting registers route constraint and ShortGuid JSON converter")]
    [Trait("Category", "Unit")]
    public void AddSecretSealRoutingRegistersRoutingServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSecretSealRouting();
        using var provider = services.BuildServiceProvider();
        var routeOptions = provider.GetRequiredService<IOptions<RouteOptions>>().Value;
        var httpJsonOptions = provider.GetRequiredService<IOptions<JsonOptions>>().Value;

        // Assert
        routeOptions.ConstraintMap["ShortGuid"].Should().Be<ShortGuidRouteConstraint>();
        httpJsonOptions.SerializerOptions.Converters.Should().ContainSingle(
            converter => converter.GetType() == typeof(ShortGuidJsonConverter));
    }

    [Fact(DisplayName = "AddSecretSealCaching throws when services are null")]
    [Trait("Category", "Unit")]
    public void AddSecretSealCachingWhenServicesAreNullThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act
        Action act = () => _ = ServiceCollectionCachingExtensions.AddSecretSealCaching(services);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "AddSecretSealCaching registers output cache options")]
    [Trait("Category", "Unit")]
    public void AddSecretSealCachingRegistersOutputCacheOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSecretSealCaching();
        using var provider = services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<IOptions<OutputCacheOptions>>().Value.Should().NotBeNull();
    }

    [Fact(DisplayName = "AddSecretSealApplicationServices throws when services are null")]
    [Trait("Category", "Unit")]
    public void AddSecretSealApplicationServicesWhenServicesAreNullThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;
        var configuration = CreateConfiguration();

        // Act
        Action act = () => _ = ServiceCollectionNotesServicesExtensions.AddSecretSealApplicationServices(services, configuration);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "AddSecretSealApplicationServices throws when configuration is null")]
    [Trait("Category", "Unit")]
    public void AddSecretSealApplicationServicesWhenConfigurationIsNullThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfiguration configuration = null!;

        // Act
        Action act = () => _ = services.AddSecretSealApplicationServices(configuration);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "AddSecretSealApplicationServices registers in-memory application services")]
    [Trait("Category", "Unit")]
    public void AddSecretSealApplicationServicesWhenStorageIsInMemoryRegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(("Storage:Mode", "InMemory"));

        services.AddSecretSealOptions(configuration);

        // Act
        services.AddSecretSealApplicationServices(configuration);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        // Assert
        scope.ServiceProvider.GetRequiredService<INoteValidator>().Should().BeOfType<NoteValidator>();
        scope.ServiceProvider.GetRequiredService<CreateNoteUseCase>().Should().NotBeNull();
        scope.ServiceProvider.GetRequiredService<TakeNoteUseCase>().Should().NotBeNull();
        scope.ServiceProvider.GetRequiredService<GetNoteStatsUseCase>().Should().NotBeNull();
        scope.ServiceProvider.GetRequiredService<GetRetentionPolicyUseCase>().Should().NotBeNull();
        scope.ServiceProvider.GetRequiredService<INotesHandler>().Should().BeOfType<Logic.CryptoNotesHandler>();
    }

    [Fact(DisplayName = "AddSecretSealApplicationServices rejects database mode without connection string")]
    [Trait("Category", "Unit")]
    public void AddSecretSealApplicationServicesWhenDatabaseConnectionIsMissingThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(("Storage:Mode", "Database"), ("ConnectionStrings:SecretSealDb", null));

        services.AddSecretSealOptions(configuration);

        // Act
        Action act = () => services.AddSecretSealApplicationServices(configuration);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact(DisplayName = "AddSecretSealApplicationServices registers database services")]
    [Trait("Category", "Unit")]
    public void AddSecretSealApplicationServicesWhenStorageIsDatabaseRegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(
            ("Storage:Mode", "Database"),
            ("ConnectionStrings:SecretSealDb", "Server=(localdb)\\MSSQLLocalDB;Database=SecretSealRegistration;Trusted_Connection=True;TrustServerCertificate=True;"));

        services.AddSecretSealOptions(configuration);

        // Act
        services.AddSecretSealApplicationServices(configuration);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        // Assert
        scope.ServiceProvider.GetRequiredService<INotesCleaningExecutor>().Should().NotBeNull();
        provider.GetRequiredService<INotesCleaningHandler>().Should().NotBeNull();
        scope.ServiceProvider.GetRequiredService<INoteRepository>().Should().BeOfType<NotesRepository>();
        scope.ServiceProvider.GetRequiredService<IUnitOfWork>().Should().BeOfType<EfUnitOfWork>();
        scope.ServiceProvider.GetRequiredService<INotesHandler>().Should().BeOfType<Logic.CryptoNotesHandler>();
        provider.GetServices<IHostedService>().Should().ContainSingle(
            service => service.GetType() == typeof(NotesCleanerService));
    }

    [Fact(DisplayName = "EnsureStorageCreatedIfNeededAsync throws when app is null")]
    [Trait("Category", "Unit")]
    public async Task EnsureStorageCreatedIfNeededAsyncWhenAppIsNullThrowsArgumentNullException()
    {
        // Arrange
        WebApplication app = null!;

        // Act
        Func<Task> act = () => WebApplicationStorageExtensions.EnsureStorageCreatedIfNeededAsync(app);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact(DisplayName = "EnsureStorageCreatedIfNeededAsync skips db initialization for in-memory mode")]
    [Trait("Category", "Unit")]
    public async Task EnsureStorageCreatedIfNeededAsyncWhenStorageIsInMemoryReturnsWithoutDbContext()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Storage:Mode"] = "InMemory"
        });
        await using var app = builder.Build();

        // Act
        // Act
        await app.EnsureStorageCreatedIfNeededAsync();
    }

    [Fact(DisplayName = "EnsureStorageCreatedIfNeededAsync creates configured database in database mode")]
    [Trait("Category", "Integration")]
    public async Task EnsureStorageCreatedIfNeededAsyncWhenStorageIsDatabaseCreatesDatabase()
    {
        // Arrange
        var connectionString =
            $"Server=(localdb)\\MSSQLLocalDB;Database=SecretSealEnsureCreated_{Guid.NewGuid():N};Trusted_Connection=True;TrustServerCertificate=True;";

        try
        {
            var builder = WebApplication.CreateBuilder();
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:Mode"] = "Database"
            });
            builder.Services.AddDbContext<SecretSealDbContext>(options => options.UseSqlServer(connectionString));

            await using var app = builder.Build();

            // Act
            await app.EnsureStorageCreatedIfNeededAsync();

            // Assert
            await using var context = CreateSqlServerContext(connectionString);
            (await context.Database.CanConnectAsync()).Should().BeTrue();
        }
        finally
        {
            await using var cleanupContext = CreateSqlServerContext(connectionString);
            await cleanupContext.Database.EnsureDeletedAsync();
        }
    }

    private static IConfiguration CreateConfiguration(params (string Key, string? Value)[] overrides)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Storage:Mode"] = "InMemory",
            ["Validation:MaxNoteLength"] = "15",
            ["Crypto:Key"] = "12345678901234567890123456789012",
            ["NotesCleaner:DaysToKeep"] = "14",
            ["NotesCleaner:CleanupInterval"] = "01:00:00"
        };

        foreach (var (key, value) in overrides)
        {
            settings[key] = value;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    private static SecretSealDbContext CreateSqlServerContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<SecretSealDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new SecretSealDbContext(options);
    }
}
