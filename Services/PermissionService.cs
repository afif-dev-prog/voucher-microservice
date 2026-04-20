using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using voucherMicroservice.Data;

namespace voucherMicroservice.Services
{
    public class PermissionService : IPermissionService
    {

        private readonly DataContext _context;

        public PermissionService(DataContext context)
        {
            _context = context;
        }

        public async Task<List<string>> GetUserPermissionsAsync(string userId, string role)
        {
            try
            {
                // Check tables have data
                var hasPerms = await _context.Permissions.AnyAsync();
                var hasRolePerms = await _context.RolePermissions.AnyAsync();

                if (!hasPerms || !hasRolePerms)
                    return new List<string>();

                // 1. Role permissions — use safer query without Join
                var rolePermIds = await _context.RolePermissions
                    .Where(rp => rp.role == role)
                    .Select(rp => rp.permission_id)
                    .ToListAsync();


                var rolePerms = await _context.Permissions
                    .Where(p => rolePermIds.Contains(p.id))
                    .Select(p => p.code)
                    .ToListAsync();

                // 2. User overrides — only if user_permissions has data
                var hasUserPerms = await _context.UserPermissions.AnyAsync();
                if (!hasUserPerms)
                    return rolePerms;

                var userOverrideIds = await _context.UserPermissions
                    .Where(up => up.user_id == userId && up.user_type == role)
                    .Select(up => new { up.permission_id, up.is_granted })
                    .ToListAsync();

                if (!userOverrideIds.Any())
                    return rolePerms;

                var overridePermIds = userOverrideIds.Select(u => u.permission_id).ToList();
                var overridePerms = await _context.Permissions
                    .Where(p => overridePermIds.Contains(p.id))
                    .Select(p => new { p.id, p.code })
                    .ToListAsync();

                // 3. Apply overrides
                foreach (var userPerm in userOverrideIds)
                {
                    var perm = overridePerms.FirstOrDefault(p => p.id == userPerm.permission_id);
                    if (perm == null) continue;

                    if (userPerm.is_granted && !rolePerms.Contains(perm.code))
                        rolePerms.Add(perm.code);
                    else if (!userPerm.is_granted)
                        rolePerms.Remove(perm.code);
                }

                return rolePerms;
            }
            catch (Exception ex)
            {
                // Log but never crash login
                Console.WriteLine($"[PermissionService] Error: {ex.Message}");
                return new List<string>();
            }
        }
    }
}