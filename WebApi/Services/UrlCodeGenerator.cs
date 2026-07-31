using System.Security.Cryptography;

namespace UrlTrimmer.WebApi.Services;

public sealed class UrlCodeGenerator
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string GenerateCode(int length = 7)
    {
        Span<byte> buffer = stackalloc byte[length];
        RandomNumberGenerator.Fill(buffer);

        var chars = new char[length];
        for (var index = 0; index < length; index++)
        {
            chars[index] = Alphabet[buffer[index] % Alphabet.Length];
        }

        return new string(chars);
    }
}