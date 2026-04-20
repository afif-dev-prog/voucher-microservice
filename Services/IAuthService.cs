using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using voucherMicroservice.Model;

namespace voucherMicroservice.Services
{
    public interface IAuthService
    {
        Task<AuthResult> AuthenticateAsync(string username, string password);

    }
}