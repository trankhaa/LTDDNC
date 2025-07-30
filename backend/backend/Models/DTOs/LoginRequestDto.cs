using System.ComponentModel.DataAnnotations;
using backend.Models.Entities;
using backend.Services.GoogleAuth;

namespace backend.Models.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }
        // Name can be optional or required based on your needs
        public string? Name { get; set; }
    }

    public class RegisterRequestDto
    {
        [Required]
        public string? Name { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        [Range(0, 120)]
        public int Age { get; set; }

        public int? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
    // Trong backend/Models/DTOs/AuthDtos.cs (hoặc file DTOs của bạn)
    public class GoogleLoginDataDto // Hoặc tên bạn đã đặt
    {
        [Required]
        public string GoogleId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? Name { get; set; }
    }

    public class GoogleTokenRequest
    {
        [Required]
        public string? Token { get; set; }
    }

    public class UserResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Email { get; set; }
    }

    public class GooglePayload
    {
        public string Sub { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
