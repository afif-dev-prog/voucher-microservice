using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet("/api/voucher/auth/list")]
        public async Task<IActionResult> GetAuthLogs(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string search = "",
            [FromQuery] string action = "",
            [FromQuery] string userType = "")
        {
            var query = _dataContext.AuthLog.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(a =>
                    a.user_id.ToLower().Contains(search) ||
                    (a.ip_address != null && a.ip_address.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(a => a.action == action.ToUpper());

            if (!string.IsNullOrWhiteSpace(userType))
                query = query.Where(a => a.user_type == userType.ToUpper());

            query = query.OrderByDescending(a => a.timestamp);

            int total = await query.CountAsync();
            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data,
                pagination = new
                {
                    totalCount = total,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize),
                    currentPage = pageNumber,
                    pageSize,
                    hasPrevious = pageNumber > 1,
                    hasNext = pageNumber < (int)Math.Ceiling(total / (double)pageSize)
                }
            });
        }

        // ── POST api/auth/login ───────────────
        [HttpPost("/api/voucher/auth/login")]
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
        [HttpPost("/api/voucher/auth/logout")]
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
        [HttpGet("/api/voucher/auth/me")]
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
        [HttpPost("/api/voucher/auth/validate")]
        [Authorize]
        public IActionResult Validate()
        {
            return Ok(new { success = true, valid = true });
        }

        [HttpGet("/api/voucher/auth/active-sessions")]
        public async Task<IActionResult> GetActiveSessions(
            [FromQuery] string? search = "",
            [FromQuery] string? userType = "")
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Get all LOGIN events
            var loginQuery = _dataContext.AuthLog
                .Where(a => a.action == "LOGIN" && a.session_id != null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                loginQuery = loginQuery.Where(a =>
                    a.user_id.ToLower().Contains(search) ||
                    (a.ip_address != null && a.ip_address.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(userType))
                loginQuery = loginQuery.Where(a => a.user_type == userType.ToUpper());

            var logins = await loginQuery
                .OrderByDescending(a => a.timestamp)
                .ToListAsync();

            // Get blacklisted JTIs
            var blacklistedJtis = await _dataContext.TokenBlacklist
                .Select(t => t.jti)
                .ToListAsync();

            // Filter: not blacklisted + token not expired (8hr = 28800 seconds)
            var activeSessions = logins
                .Where(l =>
                    !blacklistedJtis.Contains(l.session_id) &&
                    (now - l.timestamp) < 28800)
                .GroupBy(l => l.session_id)
                .Select(g => g.First())
                .ToList();

            return Ok(new
            {
                success = true,
                count = activeSessions.Count,
                data = activeSessions
            });
        }

        // ── POST kill session (blacklist token) ─
        [HttpPost("/api/voucher/auth/kill-session/{sessionId}")]
        public async Task<IActionResult> KillSession(
            string sessionId,
            [FromBody] KillSessionRequest request)
        {
            // Check if already blacklisted
            var exists = await _dataContext.TokenBlacklist
                .AnyAsync(t => t.jti == sessionId);

            if (exists)
                return Ok(new { success = false, message = "Session already terminated." });

            // Find the login log for context
            var loginLog = await _dataContext.AuthLog
                .FirstOrDefaultAsync(a => a.session_id == sessionId && a.action == "LOGIN");

            var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Blacklist the token
            await _dataContext.TokenBlacklist.AddAsync(new TokenBlacklist
            {
                id = Guid.NewGuid().ToString(),
                jti = sessionId,
                user_id = loginLog?.user_id,
                expires_at = now + 28800,
                revoked_at = now
            });

            // Log the kill action
            await _dataContext.AuthLog.AddAsync(new AuthLog
            {
                id = Guid.NewGuid().ToString(),
                user_id = loginLog?.user_id ?? "unknown",
                user_type = loginLog?.user_type ?? "UNKNOWN",
                action = "SESSION_KILLED",
                timestamp = now,
                session_id = sessionId,
                ip_address = HttpContext.Connection.RemoteIpAddress?.ToString(),
                user_agent = Request.Headers["User-Agent"].ToString()
            });

            await _dataContext.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Session for {loginLog?.user_id} terminated successfully."
            });
        }

        // ── POST kill all sessions for a user ─
        [HttpPost("/api/voucher/auth/kill-all/{userId}")]
        public async Task<IActionResult> KillAllSessions(
            string userId,
            [FromBody] KillSessionRequest request)
        {
            var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Get all active sessions for this user
            var userSessions = await _dataContext.AuthLog
                .Where(a => a.user_id == userId &&
                            a.action == "LOGIN" &&
                            a.session_id != null &&
                            (now - a.timestamp) < 28800)
                .Select(a => a.session_id!)
                .Distinct()
                .ToListAsync();

            var blacklistedJtis = await _dataContext.TokenBlacklist
                .Select(t => t.jti)
                .ToListAsync();

            var toKill = userSessions
                .Where(s => !blacklistedJtis.Contains(s))
                .ToList();

            if (!toKill.Any())
                return Ok(new { success = false, message = "No active sessions found." });

            foreach (var jti in toKill)
            {
                await _dataContext.TokenBlacklist.AddAsync(new TokenBlacklist
                {
                    id = Guid.NewGuid().ToString(),
                    jti = jti,
                    user_id = userId,
                    expires_at = now + 28800,
                    revoked_at = now
                });
            }

            // Log bulk kill
            await _dataContext.AuthLog.AddAsync(new AuthLog
            {
                id = Guid.NewGuid().ToString(),
                user_id = userId,
                user_type = "UNKNOWN",
                action = "ALL_SESSIONS_KILLED",
                timestamp = now,
                ip_address = HttpContext.Connection.RemoteIpAddress?.ToString(),
                user_agent = Request.Headers["User-Agent"].ToString()
            });

            await _dataContext.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"{toKill.Count} session(s) terminated for {userId}."
            });
        }
        [HttpPost("/api/voucher/auth/reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var tempPassword = "";
            var hashed = "";
            var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (request.UserType == UserRole.Student)
            {
                var user = await _dataContext.student
                    .FirstOrDefaultAsync(s => s.student_id == request.UserId);
                if (user == null) return NotFound(new { success = false, message = "Student not found." });

                tempPassword = $"swk@{request.UserId}";
                hashed = BCrypt.Net.BCrypt.HashPassword(tempPassword);
                user.password = hashed;
                user.date_update = now;
                user.must_change_password = true; // add this column to your table
            }
            else if (request.UserType == UserRole.Seller)
            {
                var user = await _dataContext.seller
                    .FirstOrDefaultAsync(s => s.username == request.UserId);
                if (user == null) return NotFound(new { success = false, message = "Seller not found." });

                tempPassword = $"Skills@{request.UserId}";
                hashed = BCrypt.Net.BCrypt.HashPassword(tempPassword);
                user.password = hashed;
                user.date_update = now;
                user.must_change_password = true;
            }
            else
            {
                var user = await _dataContext.stafflist
                    .FirstOrDefaultAsync(s => s.staff_id == request.UserId);
                if (user == null) return NotFound(new { success = false, message = "Staff not found." });

                tempPassword = $"swk@{request.UserId}";
                hashed = BCrypt.Net.BCrypt.HashPassword(tempPassword);
                user.password = hashed;
                user.date_update = now;
                user.must_change_password = true;
            }

            await _dataContext.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Temporary password set. User must change password on next login.",
                temporary_password = tempPassword // show this to admin so they can inform user
            });
        }

        [HttpPost("/api/voucher/auth/change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {

            var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest(new { success = false, message = "Passwords do not match." });

            var userId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
          ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var role = User.FindFirst("role")?.Value
                    ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Invalid token." });

            var newHashed = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            if (role == "STUDENT")
            {
                var user = await _dataContext.student
                    .FirstOrDefaultAsync(s => s.student_id == userId);
                if (user == null)
                    return NotFound(new { success = false, message = "User not found." });
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.password))
                    return BadRequest(new { success = false, message = "Current password is incorrect." });

                user.password = newHashed;
                user.date_update = now;
                user.must_change_password = false;
            }
            else if (role == "SELLER")
            {
                var user = await _dataContext.seller
                    .FirstOrDefaultAsync(s => s.username == userId);
                if (user == null)
                    return NotFound(new { success = false, message = "User not found." });
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.password))
                    return BadRequest(new { success = false, message = "Current password is incorrect." });

                user.password = newHashed;
                user.date_update = now;
                user.must_change_password = false;
            }
            else
            {
                var user = await _dataContext.stafflist
                    .FirstOrDefaultAsync(s => s.staff_id == userId);
                if (user == null)
                    return NotFound(new { success = false, message = "User not found." });
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.password))
                    return BadRequest(new { success = false, message = "Current password is incorrect." });

                user.password = newHashed;
                user.date_update = now;
                user.must_change_password = false;
            }

            await _dataContext.SaveChangesAsync();

            return Ok(new { success = true, message = "Password changed successfully." });
        }
    }


}