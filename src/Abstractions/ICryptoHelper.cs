namespace Abstractions;

/// <summary>
/// Defines methods for encrypting and decrypting text using a cryptographic algorithm.
/// </summary>
/// <remarks>Implementations of this interface should specify the encryption algorithm used and any requirements
/// or limitations, such as supported character sets or maximum input length. The interface does not prescribe a
/// specific algorithm or key management strategy; callers should consult the implementing class's documentation for
/// details on security guarantees and usage recommendations.</remarks>
public interface ICryptoHelper
{
    /// <summary>
    /// Encrypts the specified plain text using the configured encryption algorithm.
    /// </summary>
    /// <param name="plainText">The plain text string to be encrypted. Cannot be null or empty.</param>
    /// <returns>A string containing the encrypted representation of the input plain text.</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts the specified cipher text and returns the original plain text.
    /// </summary>
    /// <param name="cipherText">The encrypted text to be decrypted. Cannot be null or empty.</param>
    /// <returns>The decrypted plain text corresponding to the provided cipher text.</returns>
    string Decrypt(string cipherText);
}
