namespace Cryptography.Configuration;

/// <summary>
/// Represents configuration options for cryptographic operations.
/// </summary>
public sealed class CryptoOptions 
{
    /// <summary>
    /// AES key used for encryption and decryption, represented as a Base64-encoded string.
    /// </summary>
    public required string Key { get; init; }
}
