using System.Security.Cryptography;

namespace FormBuilder.Core.Common;

public static class SlugGenerator
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int DefaultLength = 8;

    // Generates a URL-safe short slug (default 8 chars, ~2.18 * 10^14 combinations).
    // Uses cryptographic randomness so slugs cannot be guessed.
    public static string Generate(int length = DefaultLength)
    {
        Span<byte> bytes = stackalloc byte[length];
        RandomNumberGenerator.Fill(bytes);

        var chars = new char[length];
        for (var i = 0; i < length; i++)
        {
            chars[i] = Alphabet[bytes[i] % Alphabet.Length];
        }
        return new string(chars);
    }
}
