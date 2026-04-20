using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Services
{
    public class PasswordService : IPasswordService
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, 10);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }

        public string GenerateTemporaryPassword()
        {
            // Generate 8-character alphanumeric password
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}