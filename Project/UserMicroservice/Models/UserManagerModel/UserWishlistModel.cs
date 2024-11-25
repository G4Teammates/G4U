using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using UserMicroservice.Models.CustomValidation;

namespace UserMicroservice.Models.UserManagerModel
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
        public string? ProductId { get; set; }

        /// <summary>
        /// The name of the product added to the wishlist.
        /// <br/>
        /// Tên của sản phẩm được thêm vào danh sách yêu thích.
        /// </summary>
        [WhiteSpaceValidation(ErrorMessage = "{0} cannot have leading or trailing spaces and must not contain more than one consecutive space.")]
        public string? ProductName { get; set; }

        /// <summary>
        /// The price of the product added to the wishlist.
        /// <br/>
        /// Giá của sản phẩm được thêm vào danh sách yêu thích.
        /// </summary>
        public decimal ProductPrice { get; set; }

        /// <summary>
        /// The image of the product added to the wishlist.
        /// <br/>
        /// Hình ảnh của sản phẩm được thêm vào danh sách yêu thích.
        /// </summary>
        public string? ProductImage { get; set; }
    }

}
