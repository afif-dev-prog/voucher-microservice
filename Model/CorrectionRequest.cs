using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    public class CorrectionRequest
    {
        public int student_id { get; set; }
        public int seller_id { get; set; }
        public decimal DuplicatedAmount { get; set; }
    }
}