using System.ComponentModel.DataAnnotations;
using UserMicroservice.DBContexts.Enum;

namespace UserMicroservice.Models
{
    public class UserInput
    {
        [Required(ErrorMessage = "User name is required")]
        [MaxLength(32, ErrorMessage = "Username must be less than 32 characters")]
        [RegularExpression(@"^[A-Za-z][A-Za-z0-9_]$", ErrorMessage = "User name must be letter or number")]
        public required string UserName { get => Email; set => value.Trim(); }
        [Phone(ErrorMessage = "Phone number must be correct format")]
        [MaxLength(15, ErrorMessage = "Phone number must be less than 15 digits")]
        public string? PhoneNumber { get; set; }
        [RegularExpression(@"^[\p{L}\p{M} 0-9]+$", ErrorMessage = "Display name must be letter or number")]
        [MaxLength(256, ErrorMessage = "Display name must be less than 256 characters")]
        public string? DisplayName { get; set; }
        [MaxLength(int.MaxValue, ErrorMessage = "Avatar too long, must be less than 2,147,483,647 characters")]
        public string? Avatar { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email must be correct format")]
        [MaxLength(320, ErrorMessage = "Email must be less than 320 characters")]
        public required string Email { get=>Email; set =>value.Trim(); }

        public string? NormalizedEmail => Email?.ToUpper();
        public string? NormalizedUserName => UserName?.ToUpper();

        public required UserStatus Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
