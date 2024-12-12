using UserMicroservice.DBContexts.Enum;

namespace UserMicroservice.Models.AuthModel
{
    public class LoginResponseModel
    {
        public string? Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public string Avatar { get; set; } = "https://static.vecteezy.com/system/resources/previews/020/911/747/non_2x/user-profile-icon-profile-avatar-user-icon-male-icon-face-icon-profile-icon-free-png.png";
        public string? Role { get; set; }
        public string? Token { get; set; }
        public string? LoginType { get; set; }
        public bool IsRememberMe { get; set; } = false;
    }
}
