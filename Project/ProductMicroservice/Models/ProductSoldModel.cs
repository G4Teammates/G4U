using System.ComponentModel.DataAnnotations;

namespace ProductMicroservice.Models
{
    public class ProductSoldModel
    {
        /// <summary>
        /// The unique identifier of the product.
        /// <br/>
        /// Mã định danh duy nhất của sản phẩm.
        /// </summary>
        public required string ProductId { get; set; }


        /// <summary>
        /// The quantity of the product ordered.
        /// <br/>
        /// Số lượng sản phẩm được đặt hàng.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "The {0} must be greater than {1}")]
        public int Quantity { get; set; }

    }
}
