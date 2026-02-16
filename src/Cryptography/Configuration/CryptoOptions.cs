using System.ComponentModel.DataAnnotations;

namespace Cryptography.Configuration;

/// <summary>
/// Represents configuration options for cryptographic operations.
/// </summary>
public sealed class CryptoOptions
{
    /// <summary>
    /// AES key used for encryption and decryption.
    /// Provide either a 32-byte raw UTF-8 string or a Base64/Base64url string that decodes to 32 bytes (AES-256).
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public required string Key { get; init; }
}
