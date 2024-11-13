using MongoDB.Bson.Serialization.Attributes;

namespace Client.Models.ProductDTO
{
    public class UserDisLikesModel
    {
        [BsonElement("userName")]
        public string UserName { get; set; }
    }
}
