namespace Client.Models.UserDTO
{
    public class WishlistModel
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
