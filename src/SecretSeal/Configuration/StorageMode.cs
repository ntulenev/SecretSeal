namespace SecretSeal.Configuration;

/// <summary>
/// Specifies the available storage modes for persisting data within the application.
/// </summary>
/// <remarks>Use this enumeration to select between in-memory storage, which keeps data only for the lifetime of
/// the process, and database storage, which persists data across application restarts. The choice of storage mode may
/// affect performance, scalability, and durability.</remarks>
internal enum StorageMode
{
    InMemory = 0,
    Database = 1
}
