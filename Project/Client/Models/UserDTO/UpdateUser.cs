namespace Client.Models.UserDTO
{
    public class UpdateUser
    {
        public required string Username { get; set; }
        public string NormalizedUsername => Username.ToUpper();
        public string? PhoneNumber { get; set; }
        public string? DisplayName { get; set; }
    }
}
