using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    [Table("user_permissions")]
    public class UserPermission
    {
        [Key]
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string user_id { get; set; } = string.Empty;
        public string user_type { get; set; } = string.Empty;
        public string permission_id { get; set; } = string.Empty;
        public bool is_granted { get; set; } = true;
        public string? set_by { get; set; }
        public long? set_at { get; set; }

        [ForeignKey("permission_id")]
        public Permission? Permission { get; set; }
    }
}