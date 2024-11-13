using MongoDB.Bson.Serialization.Attributes;

namespace CommentMicroservice.DBContexts.Entities
{
    public class UserDisLikes
    {
        [BsonElement("userName")]
        public string UserName { get; set; }
    }
}
