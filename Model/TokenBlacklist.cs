using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    [Table("token_blacklist")]
    public class TokenBlacklist
    {
        [Key]
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string jti { get; set; } = string.Empty;
        public string? user_id { get; set; }
        public long? expires_at { get; set; }
        public long? revoked_at { get; set; }
    }
}