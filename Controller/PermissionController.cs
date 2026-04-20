using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using voucherMicroservice.Data;
using voucherMicroservice.Model;

namespace voucherMicroservice.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionController : ControllerBase
    {
        private readonly DataContext _db;
        public PermissionController(DataContext db) { _db = db; }

        // ── GET all permissions ───────────────
        [HttpGet("/api/voucher/permissions/list")]
        public async Task<IActionResult> GetPermissions()
        {
            var perms = await _db.Permissions
                .OrderBy(p => p.module)
                .ThenBy(p => p.label)
                .ToListAsync();
            return Ok(new { success = true, data = perms });
        }

        // ── GET role permissions ──────────────
        [HttpGet("/api/voucher/permissions/role/{role}")]
        public async Task<IActionResult> GetRolePermissions(string role)
        {
            var granted = await _db.RolePermissions
                .Where(rp => rp.role == role)
                .Select(rp => rp.permission_id)
                .ToListAsync();
            return Ok(new { success = true, data = granted });
        }

        // ── PUT role permissions (replace all) ─
        [HttpPut("/api/voucher/permissions/role/{role}")]
        public async Task<IActionResult> SetRolePermissions(
            string role, [FromBody] SetRolePermissionsRequest request)
        {
            // Remove existing
            var existing = _db.RolePermissions.Where(rp => rp.role == role);
            _db.RolePermissions.RemoveRange(existing);

            // Add new
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            foreach (var permId in request.PermissionIds)
            {
                await _db.RolePermissions.AddAsync(new RolePermission
                {
                    id = Guid.NewGuid().ToString(),
                    role = role,
                    permission_id = permId,
                    granted_by = request.GrantedBy,
                    granted_at = now
                });
            }

            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Role permissions updated." });
        }

        // ── GET user overrides ────────────────
        [HttpGet("/api/voucher/permissions/user/{userId}")]
        public async Task<IActionResult> GetUserPermissions(string userId)
        {
            var overrides = await _db.UserPermissions
                .Where(up => up.user_id == userId)
                .Join(_db.Permissions,
                      up => up.permission_id,
                      p => p.id,
                      (up, p) => new
                      {
                          up.id,
                          up.permission_id,
                          p.code,
                          p.label,
                          p.module,
                          up.is_granted,
                          up.set_by,
                          up.set_at
                      })
                .ToListAsync();
            return Ok(new { success = true, data = overrides });
        }

        // ── PUT user override ─────────────────
        [HttpPut("/api/voucher/permissions/user/{userId}")]
        public async Task<IActionResult> SetUserPermissions(
            string userId, [FromBody] SetUserPermissionsRequest request)
        {
            // Remove existing overrides for this user
            var existing = _db.UserPermissions.Where(up => up.user_id == userId);
            _db.UserPermissions.RemoveRange(existing);

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            foreach (var override_ in request.Overrides)
            {
                await _db.UserPermissions.AddAsync(new UserPermission
                {
                    id = Guid.NewGuid().ToString(),
                    user_id = userId,
                    user_type = request.UserType,
                    permission_id = override_.PermissionId,
                    is_granted = override_.IsGranted,
                    set_by = request.SetBy,
                    set_at = now
                });
            }

            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "User permissions updated." });
        }

        // ── POST seed permissions ─────────────
        [HttpPost("/api/voucher/permissions/seed")]
        public async Task<IActionResult> SeedPermissions()
        {
            var existing = await _db.Permissions.AnyAsync();
            if (existing)
                return Ok(new { success = false, message = "Permissions already seeded." });

            var permissions = new List<Permission>
{
    // STUDENT module
    new() { id = Guid.NewGuid().ToString(), code = "VIEW_BALANCE",
            label = "View Balance",       module = "STUDENT" },
    new() { id = Guid.NewGuid().ToString(), code = "VIEW_HISTORY",
            label = "View History",       module = "STUDENT" },
    new() { id = Guid.NewGuid().ToString(), code = "MANAGE_STUDENTS",
            label = "Manage Students",    module = "STUDENT" },

    // SELLER module
    new() { id = Guid.NewGuid().ToString(), code = "SCAN_TO_PAY",
            label = "Scan to Pay",        module = "SELLER"  },
    new() { id = Guid.NewGuid().ToString(), code = "VIEW_SELLER_HISTORY",
            label = "View Seller History",module = "SELLER"  },
    new() { id = Guid.NewGuid().ToString(), code = "CLAIM_VOUCHER",
            label = "Claim Voucher",      module = "SELLER"  },
    new() { id = Guid.NewGuid().ToString(), code = "MANAGE_SELLERS",
            label = "Manage Sellers",     module = "SELLER"  },

    // VOUCHER module
    new() { id = Guid.NewGuid().ToString(), code = "MANAGE_VOUCHER",
            label = "Credit Voucher",     module = "VOUCHER" },
    new() { id = Guid.NewGuid().ToString(), code = "VIEW_FLOAT",
            label = "View Float",         module = "VOUCHER" },
    new() { id = Guid.NewGuid().ToString(), code = "MANAGE_FLOAT",
            label = "Float Management",   module = "VOUCHER" },

    // REPORTS module
    new() { id = Guid.NewGuid().ToString(), code = "VIEW_REPORTS",
            label = "View Reports",       module = "REPORTS" },

    // SYSTEM module
    new() { id = Guid.NewGuid().ToString(), code = "MANAGE_STAFF",
            label = "Manage Staff",       module = "SYSTEM"  },
    new() { id = Guid.NewGuid().ToString(), code = "MANAGE_PERMISSIONS",
            label = "System Settings",    module = "SYSTEM"  },
    new() { id = Guid.NewGuid().ToString(), code = "VIEW_AUTH_LOG",
            label = "View Auth Log",      module = "SYSTEM"  },
};
            await _db.Permissions.AddRangeAsync(permissions);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = $"{permissions.Count} permissions seeded." });
        }
    }
}