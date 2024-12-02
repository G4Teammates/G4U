using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using static Client.Models.Enum.UserEnum.User;


namespace Client.Models.UserDTO
{
    public class UpdateUser
    {
        public required string Id { get; set; }

        // Chỉ cần bắt buộc Id, các thuộc tính khác có thể là null
        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(320, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string? Username { get; set; }

        [StringLength(15, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 7)]
        [Phone(ErrorMessage = "The {0} field is not a valid phone number.")]
        public string? PhoneNumber { get; set; }

        [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [RegularExpression(@"^(?!.*\s{2})[\w\s]*$", ErrorMessage = "The {0} must not contain consecutive spaces.")]
        public string? DisplayName { get; set; }


        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(320, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [EmailAddress(ErrorMessage = "The {0} field is not a valid e-mail address.")]
        public string? Email { get; set; }

        [Url(ErrorMessage = "The {0} field is not a valid URL.")]
        [MaxLength(2048)]
        public string Avatar { get; set; } = "https://static.vecteezy.com/system/resources/previews/020/911/747/non_2x/user-profile-icon-profile-avatar-user-icon-male-icon-face-icon-profile-icon-free-png.png";

        public UserRole? Role { get; set; } = UserRole.User;

        public UserStatus? Status { get; set; } = UserStatus.Inactive;

        public EmailStatus? EmailConfirmation { get; set; } = EmailStatus.Unconfirmed;

        //public DateTime CreateAt { get; set; }

        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public IFormFile? AvatarFile { get; set; }
        [JsonIgnore]
        public string? AvatarName { get; set; }
    }

}
