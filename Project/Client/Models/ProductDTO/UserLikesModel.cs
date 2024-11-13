using MongoDB.Bson.Serialization.Attributes;

namespace Client.Models.ProductDTO
{
    public class UserLikesModel
    {
        [BsonElement("userName")]
        public string UserName { get; set; }
    }
}
