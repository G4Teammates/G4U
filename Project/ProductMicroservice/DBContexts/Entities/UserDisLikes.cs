using MongoDB.Bson.Serialization.Attributes;

namespace ProductMicroservice.DBContexts.Entities
{
    public class UserDisLikes
    {
        [BsonElement("userName")]
        public string UserName { get; set; }
    }
}
