using static Client.Models.Enum.UserEnum.User;


namespace Client.Models.UserDTO
{
    public class UpdateUser
    {
        public required string Id { get; set; }

        // Chỉ cần bắt buộc Id, các thuộc tính khác có thể là null
        public string? Username { get; set; }

        public string NormalizedUsername => Username != null ? Username.ToUpper() : string.Empty;

        public string? PhoneNumber { get; set; }

        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string NormalizedEmail => Email != null ? Email.ToUpper() : string.Empty;

        public string Avatar { get; set; } = "https://static.vecteezy.com/system/resources/previews/020/911/747/non_2x/user-profile-icon-profile-avatar-user-icon-male-icon-face-icon-profile-icon-free-png.png";

        public UserRole? Role { get; set; } = UserRole.User;

    }
}
