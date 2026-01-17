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

        Span<byte> iv = stackalloc byte[IvSizeBytes];
        RandomNumberGenerator.Fill(iv);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv.ToArray(); // aes.IV expects byte[]
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        
        using ICryptoTransform encryptor = aes.CreateEncryptor();

        var inputBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

        // Store: IV || CIPHERTEXT
        var combined = new byte[IvSizeBytes + cipherBytes.Length];
        iv.CopyTo(combined.AsSpan(0, IvSizeBytes));
        Buffer.BlockCopy(cipherBytes, 0, combined, IvSizeBytes, cipherBytes.Length);

        return Convert.ToBase64String(combined);
    }

    private static string DecryptWithIvPrefix(string combinedBase64, CryptoOptions options)
    {
        var key = GetKeyBytes(options);

        var combined = Convert.FromBase64String(combinedBase64);
        if (combined.Length <= IvSizeBytes)
            throw new CryptographicException("Encrypted payload is too short.");

        var iv = new byte[IvSizeBytes];
        Buffer.BlockCopy(combined, 0, iv, 0, IvSizeBytes);

        var cipher = new byte[combined.Length - IvSizeBytes];
        Buffer.BlockCopy(combined, IvSizeBytes, cipher, 0, cipher.Length);

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
            throw new InvalidOperationException("Crypto:Key is not configured.");

        var keyBytes = Encoding.UTF8.GetBytes(options.Key);

        if (keyBytes.Length != KeySizeBytes)
            throw new InvalidOperationException(
                $"Crypto:Key must be exactly {KeySizeBytes} bytes for AES-256. " +
                $"Current: {keyBytes.Length} bytes.");

        return keyBytes;
    }


    private const int IvSizeBytes = 16; // AES block size = 128 bits
    private const int KeySizeBytes = 32; // AES-256
    private readonly CryptoOptions _options;
}
#pragma warning restore CA5401