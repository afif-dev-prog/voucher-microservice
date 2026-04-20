using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    [Table("auth_log")]
    public class AuthLog
    {
        [Key]
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string user_id { get; set; } = string.Empty;
        public string user_type { get; set; } = string.Empty;
        public string action { get; set; } = string.Empty;
        public string? ip_address { get; set; }
        public string? user_agent { get; set; }
        public int timestamp { get; set; }
        public string? session_id { get; set; }
    }
}