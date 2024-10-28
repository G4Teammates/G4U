using System.ComponentModel.DataAnnotations;

namespace OrderMicroservice.Models.OrderModel
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

        /// <summary>
        /// Name of product
        /// <br/>
        /// Tên của sản phẩm
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public required string ProductName { get; set; }

        /// <summary>
        /// Unit price of each product
        /// <br/>
        /// Giá của 1 sản phẩm
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "The {0} must be greater or equal than {1}")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "The {0} must have no more than 2 decimal places")]
        public decimal Price { get; set; }

        /// <summary>
        /// Total price of each order include many product
        /// <br/>
        /// Tổng giá của mỗi hóa đơn bao gồm nhiều sản phẩm
        /// <br/>
        /// <see cref="TotalPrice"/> = <see cref="Price"/> * <see cref="Quantity"/>
        /// </summary>
        public decimal TotalPrice => Price * Quantity;

        /// <summary>
        /// The total profit of publisher on each product. Publisher receive 90% total price product
        /// <br/>
        /// Tổng lợi nhuận của người đăng game trên mỗi sản phẩm. Người đăng game nhận 90% tổng giá trị của sản phẩm
        /// <br/>
        /// <see cref="TotalProfit"/> = <see cref="TotalPrice"/> * <see cref="0.9"/>
        /// </summary>
        public decimal TotalProfit => TotalPrice * (decimal)0.9;

        /// <summary>
        /// The id of publisher
        /// <br/>
        /// Mã định danh của người đăng game
        /// </summary>
        public required string PublisherId { get; set; }
        /// <summary>
        /// Username of publisher
        /// <br/>
        /// Username của người đăng game
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public required string PublisherName { get; set; }

        /// <summary>
        /// The quantity of the product ordered.
        /// <br/>
        /// Số lượng sản phẩm được đặt hàng.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "The {0} must be greater than {1}")]
        public int Quantity { get; set; }
    }
}
