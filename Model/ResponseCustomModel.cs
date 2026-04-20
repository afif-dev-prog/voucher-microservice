using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    public class ResponseCustomModel<T>
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public int Count { get; set; }
        public T Data { get; set; }
    }

    public class BulkCreditResult
    {
        public int TotalRequested { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> SuccessfulIds { get; set; } = new();
        public List<FailedCredit> FailedRecords { get; set; } = new();
    }

    public class FailedCredit
    {
        public string StudentId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}