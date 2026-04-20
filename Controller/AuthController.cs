using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using voucherMicroservice.Data;
using voucherMicroservice.Model;
using voucherMicroservice.Services;

namespace voucherMicroservice.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly DataContext _dataContext;

        public AuthController(
            IAuthService authService,
            DataContext dataContext)
        {
            _authService = authService;
            _dataContext = dataContext;
        }

        // ── POST api/auth/login ───────────────
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Username and password are required."
                });
            }

            var result = await _authService.AuthenticateAsync(
                request.Username.Trim(),
                request.Password);

            if (!result.Success)
                return Unauthorized(new
                {
                    success = false,
                    message = result.Message
                });

            return Ok(new
            {
                success = true,
                message = result.Message,
                token = result.Token,
                user = result.UserInfo
            });
        }

        // ── POST api/auth/logout ──────────────
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Get JTI from token claims
                var jti = User.FindFirst("jti")?.Value
                          ?? User.FindFirst(
                               System.IdentityModel.Tokens.Jwt
                                     .JwtRegisteredClaimNames.Jti)?.Value;
                var userId = User.FindFirst("sub")?.Value
                          ?? User.FindFirst(
                               System.IdentityModel.Tokens.Jwt
                                     .JwtRegisteredClaimNames.Sub)?.Value;

                if (!string.IsNullOrEmpty(jti))
                {
                    // Blacklist the token
                    var blacklisted = new TokenBlacklist
                    {
                        jti = jti,
                        user_id = userId,
                        expires_at = DateTimeOffset.UtcNow.AddHours(8)
                                                   .ToUnixTimeSeconds(),
                        revoked_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    };

                    await _dataContext.TokenBlacklist.AddAsync(blacklisted);
                    await _dataContext.SaveChangesAsync();
                }

                return Ok(new { success = true, message = "Logged out successfully." });
            }
            catch (Exception ex)
            {
                return Ok(new { success = true, message = "Logged out." });
            }
        }

        // ── GET api/auth/me ───────────────────
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var userId = User.FindFirst("sub")?.Value;
            var name = User.FindFirst("name")?.Value;
            var role = User.FindFirst("role")?.Value;
            var perms = User.FindFirst("permissions")?.Value;

            return Ok(new
            {
                success = true,
                data = new
                {
                    user_id = userId,
                    name,
                    role,
                    permissions = perms
                }
            });
        }

        // ── POST api/auth/validate ────────────
        // Lightweight endpoint — Angular can call this
        // to check if token is still valid
        [HttpPost("validate")]
        [Authorize]
        public IActionResult Validate()
        {
            return Ok(new { success = true, valid = true });
        }
    }
}