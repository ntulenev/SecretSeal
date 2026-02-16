using Cryptography.Configuration;

using FluentAssertions;

using Microsoft.Extensions.Options;

using System.Security.Cryptography;

namespace Cryptography.Tests;

public sealed class CryptoHelperTests
{
    [Fact(DisplayName = "Constructor throws when options are null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenOptionsIsNullThrowsArgumentNullException()
    {
        // Arrange
        IOptions<CryptoOptions> options = null!;

        // Act
        Action act = () => _ = new CryptoHelper(options);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Encrypt throws when plain text is null")]
    [Trait("Category", "Unit")]
    public void EncryptWhenPlainTextIsNullThrowsArgumentNullException()
    {
        // Arrange
        var helper = CreateHelper();
        string plainText = null!;

        // Act
        Action act = () => _ = helper.Encrypt(plainText);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Decrypt throws when cipher text is null")]
    [Trait("Category", "Unit")]
    public void DecryptWhenCipherTextIsNullThrowsArgumentNullException()
    {
        // Arrange
        var helper = CreateHelper();
        string cipherText = null!;

        // Act
        Action act = () => _ = helper.Decrypt(cipherText);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Encrypt throws when key is not configured")]
    [Trait("Category", "Unit")]
    public void EncryptWhenKeyIsNullOrWhitespaceThrowsInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(new CryptoOptions { Key = "   " });
        var helper = new CryptoHelper(options);

        // Act
        Action act = () => _ = helper.Encrypt("data");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact(DisplayName = "Encrypt throws when key length is not 32 bytes")]
    [Trait("Category", "Unit")]
    public void EncryptWhenKeyLengthIsInvalidThrowsInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(new CryptoOptions { Key = "short-key" });
        var helper = new CryptoHelper(options);

        // Act
        Action act = () => _ = helper.Encrypt("data");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact(DisplayName = "Decrypt throws when payload is too short")]
    [Trait("Category", "Unit")]
    public void DecryptWhenPayloadIsTooShortThrowsCryptographicException()
    {
        // Arrange
        var helper = CreateHelper();
        var shortPayload = Convert.ToBase64String(new byte[10]);

        // Act
        Action act = () => _ = helper.Decrypt(shortPayload);

        // Assert
        act.Should().Throw<CryptographicException>();
    }

    [Fact(DisplayName = "Encrypt returns base64 payload that decrypts to the original text")]
    [Trait("Category", "Unit")]
    public void EncryptThenDecryptReturnsOriginalPlainText()
    {
        // Arrange
        var helper = CreateHelper();
        var plainText = "secret note";

        // Act
        var cipherText = helper.Encrypt(plainText);
        var decrypted = helper.Decrypt(cipherText);

        // Assert
        cipherText.Should().NotBeNullOrWhiteSpace();
        decrypted.Should().Be(plainText);
    }

    [Fact(DisplayName = "Encrypt/Decrypt works with Base64 key")]
    [Trait("Category", "Unit")]
    public void EncryptDecryptWithBase64Key()
    {
        // Arrange
        var base64Key = Convert.ToBase64String(CreateKeyBytes());
        var options = Options.Create(new CryptoOptions { Key = base64Key });
        var helper = new CryptoHelper(options);
        var plainText = "secret note";

        // Act
        var cipherText = helper.Encrypt(plainText);
        var decrypted = helper.Decrypt(cipherText);

        // Assert
        decrypted.Should().Be(plainText);
    }

    [Fact(DisplayName = "Encrypt/Decrypt works with Base64url key without padding")]
    [Trait("Category", "Unit")]
    public void EncryptDecryptWithBase64UrlKeyWithoutPadding()
    {
        // Arrange
        var base64 = Convert.ToBase64String(CreateKeyBytes());
        var base64Url = base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var options = Options.Create(new CryptoOptions { Key = base64Url });
        var helper = new CryptoHelper(options);
        var plainText = "secret note";

        // Act
        var cipherText = helper.Encrypt(plainText);
        var decrypted = helper.Decrypt(cipherText);

        // Assert
        decrypted.Should().Be(plainText);
    }

    [Fact(DisplayName = "Encrypt throws when Base64 key decodes to wrong length")]
    [Trait("Category", "Unit")]
    public void EncryptWhenBase64KeyDecodesToWrongLengthThrowsInvalidOperationException()
    {
        // Arrange
        var wrongLengthBytes = new byte[16];
        var base64Key = Convert.ToBase64String(wrongLengthBytes);
        var options = Options.Create(new CryptoOptions { Key = base64Key });
        var helper = new CryptoHelper(options);

        // Act
        Action act = () => _ = helper.Encrypt("data");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact(DisplayName = "Encrypt throws when Base64 key is malformed")]
    [Trait("Category", "Unit")]
    public void EncryptWhenBase64KeyIsMalformedThrowsInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(new CryptoOptions { Key = "not-base64**" });
        var helper = new CryptoHelper(options);

        // Act
        Action act = () => _ = helper.Encrypt("data");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact(DisplayName = "Encrypt produces different outputs for the same input")]
    [Trait("Category", "Unit")]
    public void EncryptSameInputProducesDifferentCipherText()
    {
        // Arrange
        var helper = CreateHelper();
        var plainText = "secret note";

        // Act
        var first = helper.Encrypt(plainText);
        var second = helper.Encrypt(plainText);

        // Assert
        first.Should().NotBe(second);
    }

    private static CryptoHelper CreateHelper()
    {
        var options = Options.Create(new CryptoOptions
        {
            Key = "12345678901234567890123456789012"
        });

        return new CryptoHelper(options);
    }

    private static byte[] CreateKeyBytes()
    {
        var bytes = new byte[32];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = (byte)(i + 1);
        }

        return bytes;
    }
}
