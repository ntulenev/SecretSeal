using Microsoft.EntityFrameworkCore;

namespace Storage.Tests;

internal sealed class SqlServerTestDatabase : IAsyncDisposable
{
    private SqlServerTestDatabase(string connectionString, SecretSealDbContext context)
    {
        _connectionString = connectionString;
        Context = context;
    }

    public SecretSealDbContext Context { get; }

    public static async Task<SqlServerTestDatabase> CreateAsync()
    {
        var connectionString =
            $"Server=(localdb)\\MSSQLLocalDB;Database=SecretSealTests_{Guid.NewGuid():N};Trusted_Connection=True;TrustServerCertificate=True;";

        var context = CreateContext(connectionString);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        return new SqlServerTestDatabase(connectionString, context);
    }

    public SecretSealDbContext CreateContext() => CreateContext(_connectionString);

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();

        await using var cleanupContext = CreateContext(_connectionString);
        await cleanupContext.Database.EnsureDeletedAsync();
    }

    private static SecretSealDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<SecretSealDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new SecretSealDbContext(options);
    }

    private readonly string _connectionString;
}
