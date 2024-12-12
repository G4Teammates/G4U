namespace Client.Models.AuthenModel
{
    public class UserClaimModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
        public string Role { get; set; }
        public string LoginType { get; set; }
        public string IsRememberMe { get; set; }
    }
}
