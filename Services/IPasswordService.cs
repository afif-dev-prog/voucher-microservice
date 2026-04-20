using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Services
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        string GenerateTemporaryPassword();
    }
}