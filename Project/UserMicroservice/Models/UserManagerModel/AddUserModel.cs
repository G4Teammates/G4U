using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;
using UserMicroservice.DBContexts.Enum;

namespace UserMicroservice.Models.UserManagerModel
{
    public class AddUserModel
    {
        public string Id { get; } = ObjectId.GenerateNewId().ToString();

        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(320, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public required string Username { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(320, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [EmailAddress(ErrorMessage = "The {0} field is not a valid e-mail address.")]
        public required string Email { get; set; }
        [Url(ErrorMessage = "The {0} field is not a valid URL.")]
        [MaxLength(2048)]
        public string? Avatar { get; set; }
        public UserRole Role { get; set; }
    }
}
