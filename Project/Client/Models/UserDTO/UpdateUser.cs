
using static Client.Models.Enum.UserEnum.User;

namespace Client.Models.UserDTO
{
    public class UpdateUser
    {
        public required string Id {  get; set; }
        public required string Username { get; set; }
        public string NormalizedUsername => Username != null ? Username.ToUpper() : string.Empty;
        public string? PhoneNumber { get; set; }
        public string? DisplayName { get; set; }
    }
}
