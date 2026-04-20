using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    // Transaction filter and pagination parameters
    public class TransactionFilterParams : PaginationParams
    {
        public string Seller { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public int? StartDate { get; set; }  // Unix timestamp
        public int? EndDate { get; set; }    // Unix timestamp
        public string TransactionType { get; set; } = string.Empty; // "credit", "debit", "all"
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string SortBy { get; set; } = "pay_date";  // Default: newest first
        public string SortOrder { get; set; } = "desc";
    }

    // Transaction DTO (cleaner response)
    public class PayHistoryDto
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string Seller { get; set; } = string.Empty;
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public decimal Amount => (Credit ?? 0) + (Debit ?? 0);  // Total amount
        public string TransactionType => Credit > 0 ? "Credit" : "Debit";
        public string Remark { get; set; } = string.Empty;
        public int PayDate { get; set; }
        public string FormattedDate => DateTimeOffset.FromUnixTimeSeconds(PayDate)
            .ToString("yyyy-MM-dd HH:mm:ss");
        public string UserUpdate { get; set; } = string.Empty;
        public string MonthCredit { get; set; } = string.Empty;
    }
}