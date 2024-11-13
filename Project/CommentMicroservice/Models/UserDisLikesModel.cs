using MongoDB.Bson.Serialization.Attributes;

namespace CommentMicroservice.Models
{
    public class UserDisLikesModel
    {
        [BsonElement("userName")]
        public string UserName { get; set; }
    }
}
