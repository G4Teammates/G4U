using System.ComponentModel.DataAnnotations;

namespace OrderMicroservice.Models
{
    /// <summary>
    /// Represents an item within an order, including product details and quantity.
    /// <br/>
    /// Đại diện cho một sản phẩm trong đơn hàng, bao gồm thông tin sản phẩm và số lượng.
    /// </summary>
    public class OrderItemModel
    {
        /// <summary>
        /// The unique identifier of the product.
        /// <br/>
        /// Mã định danh duy nhất của sản phẩm.
        /// </summary>
        public required string ProductId { get; set; }


        public required string PublisherId { get; set; }



        /// <summary>
        /// The quantity of the product ordered.
        /// <br/>
        /// Số lượng sản phẩm được đặt hàng.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "The {0} must be greater than {1}")]
        public int Quantity { get; set; }
    }
}
