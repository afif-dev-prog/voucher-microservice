using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using voucherMicroservice.Data;
using voucherMicroservice.Model;

namespace voucherMicroservice.Services
{
    public class AuthService : IAuthService
    {


        private readonly DataContext _dataContext;
        private readonly IJwtService _jwtService;
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            DataContext dataContext,
            IJwtService jwtService,
            IPermissionService permissionService,
            IHttpContextAccessor httpContextAccessor)
        {
            _dataContext = dataContext;
            _jwtService = jwtService;
            _permissionService = permissionService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResult> AuthenticateAsync(string username, string password)
        {
            // 1. Try student table
            var student = await _dataContext.student
                .FirstOrDefaultAsync(s => s.student_id == username);

            if (student != null && BCrypt.Net.BCrypt.Verify(password, student.password))
                return await BuildResult(
                    student.student_id,
                    student.student_name,
                    "STUDENT",
                    student.status ?? "Active");

            // 2. Try seller table
            var seller = await _dataContext.seller
                .FirstOrDefaultAsync(s => s.username == username);

            if (seller != null && BCrypt.Net.BCrypt.Verify(password, seller.password))
                return await BuildResult(
                    seller.username,
                    seller.s_name,
                    "SELLER",
                    "Active");

            // 3. Try stafflist (FINANCE + SUPERADMIN)
            var staff = await _dataContext.stafflist
                .FirstOrDefaultAsync(s => s.staff_id == username);

            if (staff != null && BCrypt.Net.BCrypt.Verify(password, staff.password))
            {
                var role = staff.lvl_access?.ToUpper() switch
                {
                    "SUPERADMIN" => "SUPERADMIN",
                    "FINANCE" => "FINANCE",
                    _ => "STAFF"
                };
                return await BuildResult(
                    staff.staff_id,
                    staff.s_name ?? staff.s_nickname ?? staff.staff_id,
                    role,
                    staff.staff_status ?? "Active");
            }

            // Log failed attempt
            await LogAuthAsync(username, "UNKNOWN", "LOGIN_FAILED");

            return new AuthResult
            {
                Success = false,
                Message = "Invalid credentials."
            };
        }

        // ── Build success result ──────────────
        private async Task<AuthResult> BuildResult(
            string userId, string name, string role, string status)
        {
            if (!status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Account is inactive."
                };
            }

            // ── Get permissions async ─────────
            var permissions = await _permissionService.GetUserPermissionsAsync(userId, role);
            var mustChangePassword = false;

            // ── Generate JWT with permissions ─
            var token = _jwtService.GenerateToken(userId, name, role, permissions, mustChangePassword);
            var jti = _jwtService.GetJti(token);

            // ── Log successful login ──────────
            await LogAuthAsync(userId, role, "LOGIN", jti);

            return new AuthResult
            {
                Success = true,
                Message = "Login successful.",
                Token = token,
                UserInfo = new
                {
                    user_id = userId,
                    name,
                    role,
                    permissions
                }
            };
        }

        // ── Log auth event ────────────────────
        private async Task LogAuthAsync(
            string userId,
            string userType,
            string action,
            string? sessionId = null)
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;
                var ipAddress = context?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
                var userAgent = context?.Request?.Headers["User-Agent"].ToString() ?? "unknown";

                var log = new AuthLog
                {
                    user_id = userId,
                    user_type = userType,
                    action = action,
                    ip_address = ipAddress,
                    user_agent = userAgent,
                    timestamp = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    session_id = sessionId
                };

                await _dataContext.AuthLog.AddAsync(log);
                await _dataContext.SaveChangesAsync();
            }
            catch
            {
                // Never let logging break the login flow
            }
        }

    }

    
}