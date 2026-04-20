using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    public class Floating
    {
        [Key]
        public int h_id { get; set; }
        public string? student_id { get; set; }
        public decimal credit { get; set; }
        public int pay_date { get; set; }
        public string? user_update { get; set; }
        public string? month_credit { get; set; }
    }

    public class ProceedFloat
    {
        public List<string> ids { get; set; } = [""];
        public decimal? amount { get; set; } = 0;
        public string month_credit { get; set; } = string.Empty;
        public string user_update { get; set; } = string.Empty;
    }

    public class UpdateFloat
    {
        public string student_id { get; set; } = string.Empty;
        public decimal? amount { get; set; }
        public int pay_date { get; set; }
        public string user_update { get; set; } = string.Empty;
        public string month_credit { get; set; } = string.Empty;
    }
}