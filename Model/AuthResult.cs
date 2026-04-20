using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public object? UserInfo { get; set; }
    }
}