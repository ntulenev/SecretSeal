using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Crypto config
builder.Services.Configure<CryptoOptions>(
    builder.Configuration.GetSection("Crypto"));

var app = builder.Build();

// In-memory encrypted note store (stores Base64(IV || CIPHERTEXT))
var notes = new ConcurrentDictionary<Guid, string>();

app.UseDefaultFiles();
app.UseStaticFiles();

// 1) Create a note -> returns GUID
app.MapPost("/notes", (CreateNoteRequest req, IOptions<CryptoOptions> crypto) =>
{
    var note = (req.Note ?? string.Empty).Trim();

    if (note.Length == 0)
        return Results.BadRequest(new { error = "Note must not be empty." });

    var encrypted = CryptoHelper.EncryptWithRandomIv(note, crypto.Value);

    var id = Guid.NewGuid();
    notes[id] = encrypted;

    return Results.Ok(new CreateNoteResponse(id));
});

// 2) Read once: return note and destroy it
app.MapDelete("/notes/{id:guid}", (Guid id, IOptions<CryptoOptions> crypto) =>
{
    if (!notes.TryRemove(id, out var encrypted))
        return Results.NotFound(new { error = "Note not found (or already consumed)." });

    var decrypted = CryptoHelper.DecryptWithIvPrefix(encrypted, crypto.Value);

    return Results.Ok(new { id, note = decrypted });
});

app.MapGet("/hc", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/stat", () => Results.Ok(new StatResponse(notes.Count)));

app.Run();

// =====================
// DTOs
// =====================
public sealed record CreateNoteRequest(string? Note);
public sealed record CreateNoteResponse(Guid Id);
public sealed record StatResponse(int NotesCount);

// =====================
// Crypto
// =====================
public sealed class CryptoOptions
{
    public string Key { get; init; } = default!;
}

static class CryptoHelper
{
    private const int IvSizeBytes = 16; // AES block size = 128 bits
    private const int KeySizeBytes = 32; // AES-256

    public static string EncryptWithRandomIv(string plainText, CryptoOptions options)
    {
        var key = GetKeyBytes(options);

        Span<byte> iv = stackalloc byte[IvSizeBytes];
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
        var combined = new byte[IvSizeBytes + cipherBytes.Length];
        iv.CopyTo(combined.AsSpan(0, IvSizeBytes));
        Buffer.BlockCopy(cipherBytes, 0, combined, IvSizeBytes, cipherBytes.Length);

        return Convert.ToBase64String(combined);
    }

    public static string DecryptWithIvPrefix(string combinedBase64, CryptoOptions options)
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
}
