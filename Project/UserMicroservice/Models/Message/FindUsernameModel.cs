using UserMicroservice.Models.UserManagerModel;

namespace UserMicroservice.Models.Message
{
    public class FindUsernameModel
    {
        public ICollection<UserModel> Users { get; set; }
        public ICollection<ExportProfitModel> UsersExport { get; set; }
        public ICollection<string> MissingUsers { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
