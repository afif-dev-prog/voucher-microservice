using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using voucherMicroservice.Model;

namespace voucherMicroservice.Services
{
    public interface IJwtService
    {
        // string GenerateToken(string userId, UserRole role, SystemAccessLevel? accessLevel = null);
        // ClaimsPrincipal ValidateToken(string token);
        string GenerateToken(string userId, string name, string role, List<string> permissions, bool mustChangePassword);
        string GetJti(string token);
    }
}