using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    public class Seller
    {
        [Key]
        public int? s_id { get; set; }
        public string username { get; set; } = string.Empty;
        public string s_name { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public decimal? balance { get; set; }
        public int? date_update { get; set; } = 0;
        public string firstTime { get; set; } = string.Empty;
        public int? last_password_change { get; set; } = 0;
        public string s_email { get; set; } = string.Empty;
    }

    public class CreditDebitParam
    {
        public decimal? Credit { get; set; }
        public decimal? Debit { get; set; }
    }

    public class ClaimParam
    {
        public int StartDate { get; set; }
        public int EndDate { get; set; }
    }
}