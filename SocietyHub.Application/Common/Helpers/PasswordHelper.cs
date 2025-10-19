using System;
using System.Security.Cryptography;

namespace SocietyHub.Application.Common.Helpers
{
    public static class PasswordHelper
    {
        // PBKDF2 settings
        private const int SaltSize = 16; // bytes
        private const int HashSize = 32; // bytes
        private const int Iterations = 100_000;

        public static (byte[] Hash, byte[] Salt) HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(HashSize);
            return (hash, salt);
        }

        public static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, storedSalt, Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(HashSize);
            return CryptographicOperations.FixedTimeEquals(hash, storedHash);
        }
    }
}
