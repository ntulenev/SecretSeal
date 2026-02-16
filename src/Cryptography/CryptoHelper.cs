using Abstractions;

using Cryptography.Configuration;

using Microsoft.Extensions.Options;

using System.Security.Cryptography;
using System.Text;

namespace Cryptography;

#pragma warning disable CA5401 // Do not use insecure cryptographic algorithms
/// <summary>
/// Provides methods for encrypting and decrypting text using AES-256 encryption with a randomly generated
/// initialization vector (IV).
/// </summary>
/// <remarks>This class is configured via a <see cref="CryptoOptions"/> instance, typically provided
/// through dependency injection. The encryption key must be exactly 32 bytes (256 bits) in length. Each encryption
/// operation generates a new random IV, which is prefixed to the ciphertext and required for decryption. This class
/// is not thread-safe if the underlying options are modified concurrently.</remarks>
public class CryptoHelper : ICryptoHelper
{
    /// <summary>
    /// Initializes a new instance of the CryptoHelper class using the specified cryptographic options.
    /// </summary>
    /// <param name="options">The options containing configuration settings for cryptographic operations. Cannot be null.</param>
    public CryptoHelper(IOptions<CryptoOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
    }

    /// <summary>
    /// Encrypts the specified plain text string and returns the encrypted result as a base64-encoded string.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt. Cannot be null.</param>
    /// <returns>A base64-encoded string containing the encrypted representation of the input plain text.</returns>
    public string Encrypt(string plainText)
    {
        ArgumentNullException.ThrowIfNull(plainText);

        return EncryptWithRandomIv(plainText, _options);
    }

    /// <summary>
    /// Decrypts the specified cipher text and returns the original plain text.
    /// </summary>
    /// <param name="cipherText">The encrypted text to decrypt. Cannot be null.</param>
    /// <returns>The decrypted plain text corresponding to the specified cipher text.</returns>
    public string Decrypt(string cipherText)
    {
        ArgumentNullException.ThrowIfNull(cipherText);
        return DecryptWithIvPrefix(cipherText, _options);
    }

    private static string EncryptWithRandomIv(string plainText, CryptoOptions options)
    {
        var key = GetKeyBytes(options);

        Span<byte> iv = stackalloc byte[IV_SIZE_BYTES];
        RandomNumberGenerator.Fill(iv);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv.ToArray(); // aes.IV expects byte[]
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();

        var inputBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

        // Store: IV || CIPHERTEXT
        var combined = new byte[IV_SIZE_BYTES + cipherBytes.Length];
        iv.CopyTo(combined.AsSpan(0, IV_SIZE_BYTES));
        Buffer.BlockCopy(cipherBytes, 0, combined, IV_SIZE_BYTES, cipherBytes.Length);

        return Convert.ToBase64String(combined);
    }

    private static string DecryptWithIvPrefix(string combinedBase64, CryptoOptions options)
    {
        var key = GetKeyBytes(options);

        var combined = Convert.FromBase64String(combinedBase64);
        if (combined.Length <= IV_SIZE_BYTES)
        {
            throw new CryptographicException("Encrypted payload is too short.");
        }

        var iv = new byte[IV_SIZE_BYTES];
        Buffer.BlockCopy(combined, 0, iv, 0, IV_SIZE_BYTES);

        var cipher = new byte[combined.Length - IV_SIZE_BYTES];
        Buffer.BlockCopy(combined, IV_SIZE_BYTES, cipher, 0, cipher.Length);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    private static byte[] GetKeyBytes(CryptoOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Key))
        {
            throw new InvalidOperationException("Crypto:Key is not configured.");
        }

        var keyString = options.Key.Trim();
        var keyBytes = Encoding.UTF8.GetBytes(keyString);
        if (keyBytes.Length == KEY_SIZE_BYTES)
        {
            return keyBytes;
        }

        try
        {
            var normalized = NormalizeBase64(keyString);
            var decoded = Convert.FromBase64String(normalized);
            if (decoded.Length == KEY_SIZE_BYTES)
            {
                return decoded;
            }

            throw new InvalidOperationException(
                $"Crypto:Key must be exactly {KEY_SIZE_BYTES} bytes (raw UTF-8) or Base64 that decodes to {KEY_SIZE_BYTES} bytes. " +
                $"Current decoded length: {decoded.Length} bytes.");
        }
        catch (FormatException)
        {
            // fall through to the error below
        }

        throw new InvalidOperationException(
            $"Crypto:Key must be exactly {KEY_SIZE_BYTES} bytes (raw UTF-8) or Base64 that decodes to {KEY_SIZE_BYTES} bytes. " +
            $"Current: {keyBytes.Length} bytes.");
    }

    private static string NormalizeBase64(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        var mod = s.Length % 4;
        return mod switch
        {
            0 => s,
            2 => s + "==",
            3 => s + "=",
            _ => throw new FormatException("Invalid Base64 length.")
        };
    }


    private const int IV_SIZE_BYTES = 16; // AES block size = 128 bits
    private const int KEY_SIZE_BYTES = 32; // AES-256
    private readonly CryptoOptions _options;
}
#pragma warning restore CA5401
