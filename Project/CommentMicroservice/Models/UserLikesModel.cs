using MongoDB.Bson.Serialization.Attributes;

namespace CommentMicroservice.Models
{
    public class UserLikesModel
    {
        [BsonElement("userName")]
        public string UserName { get; set; }
    }
}
