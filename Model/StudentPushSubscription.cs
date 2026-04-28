using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    [Table("push_subscriptions")]
    public class StudentPushSubscription
    {
        [Key]
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string student_id { get; set; } = string.Empty;
        public string endpoint { get; set; } = string.Empty;
        public string p256dh { get; set; } = string.Empty;
        public string auth { get; set; } = string.Empty;
        public long created_at { get; set; }
    }
}