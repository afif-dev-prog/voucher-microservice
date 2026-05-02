using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    [Table("pending_payments")]
    public class PendingPayment
    {
        [Key]
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string student_id { get; set; } = string.Empty;
        public string seller_username { get; set; } = string.Empty;
        public string seller_name { get; set; } = string.Empty;
        public decimal amount { get; set; }
        public string status { get; set; } = "pending";
        public long created_at { get; set; }
        public long? resolved_at { get; set; }
        public long expires_at { get; set; }
    }
}