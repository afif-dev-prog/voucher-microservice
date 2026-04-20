using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    [Table("role_permissions")]
    public class RolePermission
    {
        [Key]
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string role { get; set; } = string.Empty;
        public string permission_id { get; set; } = string.Empty;
        public string? granted_by { get; set; }
        public long? granted_at { get; set; }

        [ForeignKey("permission_id")]
        public Permission? Permission { get; set; }
    }
}