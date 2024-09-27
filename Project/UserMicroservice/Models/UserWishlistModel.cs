using MongoDB.Bson;

namespace UserMicroservice.Models
{
    public class UserWishlistModel
    {
        public ObjectId ProductId { get; set; }
    }
}
