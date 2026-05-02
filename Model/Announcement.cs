using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    [Table("announcements")]
    public class Announcement
    {
        [Key]
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string title { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
        public string target { get; set; } = "ALL"; // ALL, STUDENT, SELLER
        public bool send_email { get; set; } = false;
        public bool send_push { get; set; } = true;
        public string created_by { get; set; } = string.Empty;
        public long created_at { get; set; }
    }

    public class AnnouncementRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Target { get; set; } = "ALL"; // ALL, STUDENT, SELLER
        public bool SendEmail { get; set; } = false;
        public bool SendPush { get; set; } = true;
    }
}