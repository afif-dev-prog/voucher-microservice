using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using voucherMicroservice.Model;

namespace voucherMicroservice.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(string userId, string name, string role, List<string> permissions)
        {
            var key = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jti = Guid.NewGuid().ToString();

            var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, userId),
        new(JwtRegisteredClaimNames.Jti, jti),
        new("name",        name),
        new("role",        role),
        new("permissions", System.Text.Json.JsonSerializer.Serialize(permissions)) // ← add
    };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GetJti(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.Id; // reads the 'jti' claim
        }
    }
}