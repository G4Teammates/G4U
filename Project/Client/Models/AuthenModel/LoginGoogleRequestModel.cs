using Client.Models.Enum;
using System.Runtime.CompilerServices;
using static Client.Models.Enum.UserEnum.User;

namespace Client.Models.AuthenModel
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
