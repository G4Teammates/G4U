using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using static Client.Models.Enum.User;

namespace Client.Models.UserDTO
{
	public class CreateUser
	{
		[Required(ErrorMessage = "The {0} field is required.")]
		[StringLength(32, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
		[RegularExpression(@"^[a-zA-Z0-9_-]*$", ErrorMessage = "The {0} can only contain letters, numbers, underscores (_), and hyphens (-).")]
		public required string Username { get; set; }
		[Required(ErrorMessage = "The {0} field is required.")]
		[StringLength(320, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
		[EmailAddress(ErrorMessage = "The {0} field is not a valid e-mail address.")]
		public required string Email { get; set; }
		[JsonIgnore]
		public IFormFile? AvatarFile { get; set; }
        [JsonIgnore]
        public string? AvatarName { get; set; }
		public string Avatar { get; set; } = "~/image/avatar.jpeg";
		public UserRole Role { get; set; }
	}
}
