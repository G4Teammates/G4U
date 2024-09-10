using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using UserMicroService.DBContexts.Enum;

namespace UserMicroservice.DBContexts.Entities
{
    public class User : IdentityUser<Guid>
    {
        [Required(ErrorMessage = "Email is required")]
        public override string? UserName { get; set; }
        [Phone(ErrorMessage = "Phone number must be correct format")]
        [MaxLength(15, ErrorMessage = "Phone number must be less than 15 digits")]
        public override string? PhoneNumber { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email must be correct format")]
        [MaxLength(320, ErrorMessage = "Email must be less than 320 characters")]
        public override string? Email { get; set; }
        [MaxLength(256)]
        public string? DisplayName { get; set; }
        public string? Avatar { get; set; }

        public override string? NormalizedEmail => Email?.ToUpper();
        public override string? NormalizedUserName => UserName?.ToUpper();

        public required UserStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
