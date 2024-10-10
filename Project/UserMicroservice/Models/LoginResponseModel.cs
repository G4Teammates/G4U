using UserMicroService.DBContexts.Enum;
using UserMicroService.Models;

namespace UserMicroservice.Models
{
    public class LoginResponseModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
    }
}
