using MongoDB.Bson.Serialization.Attributes;

namespace ProductMicroservice.DBContexts.Entities
{
    public class Interactions
    {
        /// <summary>
        /// The number of views the product has received.
        /// <br/>
        /// Số lượt xem mà sản phẩm đã nhận được.
        /// </summary>
        [BsonElement("numberOfViews")]
        public int NumberOfViews { get; set; }

        /// <summary>
        /// The number of likes the product has received.
        /// <br/>
        /// Số lượt thích mà sản phẩm đã nhận được.
        /// </summary>
        [BsonElement("numberOfLikes")]
        public int NumberOfLikes { get; set; }


        [BsonElement("numberOfDisLikes")]
        public int NumberOfDisLikes { get; set; }


        [BsonElement("userDisLikes")]
        public ICollection<UserDisLikes>? UserDisLikes { get; set; }

        [BsonElement("userLikes")]
        public ICollection<UserLikes>? UserLikes { get; set; }
    }
}
