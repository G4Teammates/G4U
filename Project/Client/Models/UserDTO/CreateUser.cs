using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using static Client.Models.Enum.UserEnum.User;

namespace Client.Models.UserDTO
{
    public class CreateUser
	{
		[Required(ErrorMessage = "The {0} field is required.")]
		[StringLength(320, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [RegularExpression(@"^[a-zA-Z0-9_@-]*$", ErrorMessage = "The {0} can only contain letters, numbers, underscores (_), at (@) and hyphens (-).")]
        public string Username { get; set; }
		[Required(ErrorMessage = "The {0} field is required.")]
		[StringLength(320, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
		[EmailAddress(ErrorMessage = "The {0} field is not a valid e-mail address.")]
		public string Email { get; set; }
		[JsonIgnore]
		public IFormFile? AvatarFile { get; set; }
        [JsonIgnore]
        public string? AvatarName { get; set; }
		public string Avatar { get; set; } = "https://static.vecteezy.com/system/resources/previews/020/911/747/non_2x/user-profile-icon-profile-avatar-user-icon-male-icon-face-icon-profile-icon-free-png.png";
		public UserRole Role { get; set; }
	}
}
