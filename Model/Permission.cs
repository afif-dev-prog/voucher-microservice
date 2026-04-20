using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    [Table("permissions")]
    public class Permission
    {
        [Key]
        public string id { get; set; } = string.Empty;
        public string code { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
        public string module { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
    }


    // SetRolePermissionsRequest.cs
    public class SetRolePermissionsRequest
    {
        public List<string> PermissionIds { get; set; } = new();
        public string GrantedBy { get; set; } = string.Empty;
    }

    // SetUserPermissionsRequest.cs
    public class SetUserPermissionsRequest
    {
        public string UserType { get; set; } = string.Empty;
        public string SetBy { get; set; } = string.Empty;
        public List<UserPermissionOverride> Overrides { get; set; } = new();
    }

    public class UserPermissionOverride
    {
        public string PermissionId { get; set; } = string.Empty;
        public bool IsGranted { get; set; } = true;
    }
}