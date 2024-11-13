using MongoDB.Bson.Serialization.Attributes;

namespace ProductMicroservice.Models
{
    public class UserDisLikesModel
    {
        [BsonElement("userName")]
        public string UserName { get; set; }
    }
}
