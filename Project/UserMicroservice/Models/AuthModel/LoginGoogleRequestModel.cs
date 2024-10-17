using UserMicroService.DBContexts.Enum;

namespace UserMicroservice.Models.AuthModel
{
    public class LoginGoogleRequestModel
    {
        public string? Username { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? Picture { get; set; }
        public EmailStatus EmailConfirmation { get; set; }
    }
}
