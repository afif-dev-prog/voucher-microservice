using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    public class PayHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int h_id { get; set; }
        public string transaction_id { get; set; } = string.Empty;
        public string? student_id { get; set; } = string.Empty;
        public string? seller { get; set; } = string.Empty;
        public decimal? debit { get; set; }
        public decimal? credit { get; set; }
        public string? remark { get; set; } = string.Empty;
        public int pay_date { get; set; }
        public string? user_update { get; set; } = string.Empty;
        public string? month_credit { get; set; } = string.Empty;
    }
}