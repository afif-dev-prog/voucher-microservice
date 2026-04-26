using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    public class AuthDTOs
    {

    }

    // Enums
    public enum UserRole
    {
        Student = 1,
        Seller = 2,
        Staff = 3,
        Admin = 4
    }

    public enum SystemAccessLevel
    {
        ReadOnly = 1,
        Standard = 2,
        Manager = 3,
        Administrator = 4
    }

    // Authentication DTOs
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;  // Can be studentId, staffId, or sellerId

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public UserRole UserType { get; set; }  // 1=Student, 2=Seller, 3=Staff
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string FirstTime { get; set; } = string.Empty;
        public UserInfo User { get; set; }
    }

    public class UserInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public SystemAccessLevel? AccessLevel { get; set; }  // Only for staff
        public decimal? Balance { get; set; }  // For student/seller
    }

    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public UserRole UserType { get; set; }
        public string TemporaryPassword { get; set; }
    }
}