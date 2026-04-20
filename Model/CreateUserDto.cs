using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    // DTOs for user creation
    public class CreateStudentRequest
    {
        [Required]
        public string StudentId { get; set; }

        [Required]
        public string StudentName { get; set; }

        [Required]
        public string Nric { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public decimal InitialBalance { get; set; } = 0;
    }

    public class CreateSellerRequest
    {
        [Required]
        public string SellerName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public decimal InitialBalance { get; set; } = 0;
    }

    public class CreateStaffRequest
    {
        [Required]
        public string StaffId { get; set; }

        [Required]
        public string StaffName { get; set; }

        [Required]
        public string Nric { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Range(1, 4)]
        public int AccessLevel { get; set; }  // 1=ReadOnly, 2=Standard, 3=Manager, 4=Admin
    }

    public class UserCreatedResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TemporaryPassword { get; set; } = string.Empty;
        public UserRole UserType { get; set; }
    }
}