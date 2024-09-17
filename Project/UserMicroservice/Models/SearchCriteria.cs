using UserMicroService.DBContexts.Enum;

namespace UserMicroservice.Models
{
    public class SearchCriteria
    {
        public string? DisplayName { get; set; } = string.Empty;
        public UserStatus Status { get; set; } = UserStatus.Active;
        public string? Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
        public string? Username { get; set; } = string.Empty;
    }
}
