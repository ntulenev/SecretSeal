using System.ComponentModel.DataAnnotations;

namespace Cryptography.Configuration;

/// <summary>
/// Represents configuration options for cryptographic operations.
/// </summary>
public sealed class CryptoOptions
{
    /// <summary>
    /// AES key used for encryption and decryption, represented as a Base64-encoded string.
    /// Recommended: 32 bytes (AES-256).
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public required string Key { get; init; }
}
