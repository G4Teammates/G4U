using UserMicroservice.Models.Message;

namespace UserMicroservice.Models.UserManagerModel
{
    public class ExportResult
    {
        public ICollection<UserOrderModel> ExportProfits { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
