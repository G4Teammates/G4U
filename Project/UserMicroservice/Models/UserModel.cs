using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using UserMicroService.DBContexts.Enum;

namespace UserMicroService.Models
{
    public class UserModel
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(32, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [RegularExpression(@"^[a-zA-Z0-9_-]*$", ErrorMessage = "The {0} can only contain letters, numbers, underscores (_), and hyphens (-).")]
        public required string UserName { get; set; }

        public string NormalizedUserName => UserName.ToUpper();

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
            get => string.IsNullOrEmpty(_displayName) ? "User" + Id.ToString().Substring(0,8) : _displayName;
            set => _displayName = value;
        }

        [Url(ErrorMessage = "The {0} field is not a valid URL.")]
        [MaxLength(2048)]
        public string Avatar { get; set; } = "https://static.vecteezy.com/system/resources/previews/020/911/747/non_2x/user-profile-icon-profile-avatar-user-icon-male-icon-face-icon-profile-icon-free-png.png";

        public UserStatus Status { get; set; } = UserStatus.Inactive;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
