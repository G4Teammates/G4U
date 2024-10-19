namespace Client.Models.UserDTO
{
    public class ListUser
    {
        public required string Username { get; set; }
        public string NormalizedUsername => Username != null ? Username.ToUpper() : string.Empty;

        public required string Email { get; set; }
        public string NormalizedEmail => Email != null ? Email.ToUpper() : string.Empty;
        public string? PhoneNumber { get; set; }
        public string? DisplayName { get; set; }
        public string Avatar { get; set; } = "https://static.vecteezy.com/system/resources/previews/020/911/747/non_2x/user-profile-icon-profile-avatar-user-icon-male-icon-face-icon-profile-icon-free-png.png";
    }
}
