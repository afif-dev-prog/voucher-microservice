using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    [Table("notifications")]
    public class Notifications
    {
        [Key]
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string user_id { get; set; } = string.Empty;
        public string user_type { get; set; } = string.Empty; // STUDENT, SELLER
        public string type { get; set; } = string.Empty; // PAYMENT_RECEIVED, PAYMENT_DEDUCTED, ANNOUNCEMENT
        public string title { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
        public bool is_read { get; set; } = false;
        public long created_at { get; set; }
        public string? reference_id { get; set; } // payment id or announcement id
    }
}