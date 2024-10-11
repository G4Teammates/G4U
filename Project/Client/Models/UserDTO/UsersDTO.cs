using static Client.Models.Enum.User;

namespace Client.Models.UserDTO
{
	public class UsersDTO
	{
		public required string Id { get; set; }
		public required string Username { get; set; }
		public string NormalizedUsername => Username.ToUpper();
		public required string Email { get; set; }
		public string NormalizedEmail => Email.ToUpper();
		public string? PhoneNumber { get; set; }
		public string? DisplayName { get; set; }
		public string Avatar { get; set; } = "https://static.vecteezy.com/system/resources/previews/020/911/747/non_2x/user-profile-icon-profile-avatar-user-icon-male-icon-face-icon-profile-icon-free-png.png";
		public UserRole Role { get; set; } = UserRole.User;
		public float TotalProfit { get; set; } = 0;
	}
}
