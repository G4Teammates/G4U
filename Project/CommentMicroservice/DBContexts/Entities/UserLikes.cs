using MongoDB.Bson.Serialization.Attributes;

namespace CommentMicroservice.DBContexts.Entities
{
    public class UserLikes
    {
        [BsonElement("userName")]
        public string UserName { get; set; }
    }
}
