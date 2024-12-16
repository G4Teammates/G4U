using RabbitMQ.Client;

namespace OrderMicroservice.Models.UserModel
{
    public class FindUsernameModel
    {
        public ICollection<UserModel> Users { get; set; }
        public ICollection<ExportUserModel> UsersExport { get; set; }
        public ICollection<string> MissingUsers { get; set; }
    }
}
