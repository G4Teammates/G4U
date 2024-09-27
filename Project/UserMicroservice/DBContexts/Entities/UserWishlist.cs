using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UserMicroservice.DBContexts.Entities
{
    public class UserWishlist
    {
        [BsonElement("productId")]
        public ObjectId ProductId { get; set; }
    }
}
