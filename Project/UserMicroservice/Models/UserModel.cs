using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroService.DBContexts.Enum;

namespace UserMicroService.Models
{
    public class UserModel
    {
        public string Id { get; } = ObjectId.GenerateNewId().ToString();

        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(32, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [RegularExpression(@"^[a-zA-Z0-9_-]*$", ErrorMessage = "The {0} can only contain letters, numbers, underscores (_), and hyphens (-).")]
        public required string Username { get; set; }

        public string NormalizedUsername => Username.ToUpper();

        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(320, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [EmailAddress(ErrorMessage = "The {0} field is not a valid e-mail address.")]
        public required string Email { get; set; }

        public string NormalizedEmail => Email.ToUpper();

        [StringLength(15, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 7)]
        [Phone(ErrorMessage = "The {0} field is not a valid phone number.")]
        public string? PhoneNumber { get; set; }

        [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        private string? _displayName;
        public string? DisplayName
        {
            get => string.IsNullOrEmpty(_displayName) ? Username : _displayName;
            set => _displayName = value;
        }

        [Url(ErrorMessage = "The {0} field is not a valid URL.")]
        [MaxLength(2048)]
        public string Avatar { get; set; } = "https://static.vecteezy.com/system/resources/previews/020/911/747/non_2x/user-profile-icon-profile-avatar-user-icon-male-icon-face-icon-profile-icon-free-png.png";

        public UserRole Role { get; set; }

        public float TotalProfit { get; set; }

        public ICollection<UserWishlistModel>? Wishlist { get; set; }

        public UserStatus Status { get; set; } = UserStatus.Inactive;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
