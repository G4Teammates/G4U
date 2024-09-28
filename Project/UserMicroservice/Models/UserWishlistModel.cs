using MongoDB.Bson;

namespace UserMicroservice.Models
{
    /// <summary>
    /// Represents an item in the user's wishlist.
    /// <br/>
    /// Đại diện cho một sản phẩm trong danh sách yêu thích của người dùng.
    /// </summary>
    public class UserWishlistModel
    {
        /// <summary>
        /// The unique identifier of the product added to the wishlist.
        /// <br/>
        /// Mã định danh duy nhất của sản phẩm được thêm vào danh sách yêu thích.
        /// </summary>
        public ObjectId ProductId { get; set; }
    }

}
