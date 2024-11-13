using MongoDB.Bson.Serialization.Attributes;

namespace ProductMicroservice.Models
{
    public class UserLikesModel
    {
        [BsonElement("userName")]
        public string UserName { get; set; }
    }
}
