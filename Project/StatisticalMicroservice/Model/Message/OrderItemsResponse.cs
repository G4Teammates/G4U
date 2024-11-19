using System.ComponentModel.DataAnnotations;

namespace StatisticalMicroservice.Model.Message
{
    public class OrderItemsResponse
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
        /// The quantity of the product ordered.
        /// <br/>
        /// Số lượng sản phẩm được đặt hàng.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "The {0} must be greater than {1}")]
        public int Quantity { get; set; }

    }
}
