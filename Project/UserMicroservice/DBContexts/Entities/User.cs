using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using UserMicroService.DBContexts.Enum;

namespace UserMicroService.DBContexts.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string? DisplayName { get; set; }
        public string? Avatar { get; set; }
        public required UserStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
