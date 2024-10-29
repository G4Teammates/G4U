using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UserMicroservice.DBContexts.Entities
{
    public class UserWishlist
    {
        [BsonElement("productId")]
        public string? ProductId { get; set; }
        [BsonElement("productName")]
        public string? ProductName { get; set; }
        [BsonElement("productPrice")]
        public decimal ProductPrice { get; set; }
        [BsonElement("productImage")]
        public string? ProductImage { get; set; }
    }
}
