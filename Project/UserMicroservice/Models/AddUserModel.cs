using MongoDB.Bson;
using UserMicroService.DBContexts.Enum;

namespace UserMicroservice.Models
{
    public class AddUserModel
    {
        public string Id { get ;} = ObjectId.GenerateNewId().ToString();
        public required string Username { get; set; }
        public required string Email { get; set; }
        public UserRole Role { get; set; }
    }
}
