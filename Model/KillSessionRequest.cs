using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    public class KillSessionRequest
    {
        public string KilledBy { get; set; } = string.Empty;
    }
}