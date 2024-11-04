using MongoDB.Bson;
using StatisticalMicroservice.DBContexts.Entities;

namespace StatisticalMicroservice.Model
{
    public class StatisticalModel
    {
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public TotalWebsiteInfoModel TotalWebsite { get; set; }

       /* public List<UserInfoModel> TotalUser { get; set; }
*/
        public DateTime CreateAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
