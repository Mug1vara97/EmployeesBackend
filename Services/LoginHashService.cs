using System.Security.Cryptography;
using System.Text;

namespace EmployerApp.Api.Services;

public class LoginHashService : ILoginHashService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000; 

    public string HashLogin(string login)
    {
        if (string.IsNullOrEmpty(login))
            throw new ArgumentException("Login cannot be null or empty", nameof(login));

        var normalizedLogin = login.Trim().ToLowerInvariant();

        var salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        var hash = PBKDF2(normalizedLogin, salt, Iterations, HashSize);

        var saltAndHash = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, saltAndHash, 0, SaltSize);
        Array.Copy(hash, 0, saltAndHash, SaltSize, HashSize);

        return Convert.ToBase64String(saltAndHash);
    }

    public bool VerifyLogin(string login, string hashedLogin)
    {
        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(hashedLogin))
            return false;

        try
        {
            var normalizedLogin = login.Trim().ToLowerInvariant();

            var saltAndHash = Convert.FromBase64String(hashedLogin);
            
            if (saltAndHash.Length != SaltSize + HashSize)
                return false;

            var salt = new byte[SaltSize];
            var hash = new byte[HashSize];
            Array.Copy(saltAndHash, 0, salt, 0, SaltSize);
            Array.Copy(saltAndHash, SaltSize, hash, HashSize, HashSize);

            var testHash = PBKDF2(normalizedLogin, salt, Iterations, HashSize);

            return SlowEquals(hash, testHash);
        }
        catch
        {
            return false;
        }
    }

    private static byte[] PBKDF2(string password, byte[] salt, int iterations, int outputBytes)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(outputBytes);
    }

    private static bool SlowEquals(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;

        var diff = (uint)a.Length ^ (uint)b.Length;
        for (var i = 0; i < a.Length && i < b.Length; i++)
        {
            diff |= (uint)(a[i] ^ b[i]);
        }

        return diff == 0;
    }
}
