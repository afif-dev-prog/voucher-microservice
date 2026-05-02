using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    public class InitiatePaymentRequest
    {
        public string StudentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string SellerUsername { get; set; } = string.Empty; // ← add this
    }
}