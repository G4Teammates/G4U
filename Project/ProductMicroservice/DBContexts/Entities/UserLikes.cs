using MongoDB.Bson.Serialization.Attributes;

namespace ProductMicroservice.DBContexts.Entities
{
    public class UserLikes
    {
        [BsonElement("userName")]
        public string UserName { get; set; }
    }
}
