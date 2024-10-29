using OrderMicroservice.Models.ProductModel.Enum;
using System.ComponentModel.DataAnnotations;

namespace OrderMicroservice.Models.ProductModel
{
    /// <summary>
    /// Represents a product in the system.
    /// <br/>
    /// Đại diện cho một sản phẩm trong hệ thống.
    /// </summary>
    public class ProductModel
    {
        /// <summary>
        /// Unique identifier for the product.
        /// <br/>
        /// Định danh duy nhất cho sản phẩm.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// The name of the product.
        /// <br/>
        /// Tên của sản phẩm.
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public required string Name { get; set; }

        /// <summary>
        /// The original price of the product before any discounts are applied.
        /// <br/>
        /// Giá gốc của sản phẩm trước khi áp dụng bất kỳ giảm giá nào.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "The {0} must be greater or equal than {1}")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "The {0} must have no more than 2 decimal places")]
        public decimal Price { get; set; }

        /// <summary>
        /// The number of products sold.
        /// <br/>
        /// Số sản phẩm đã bán được.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "The {0} must be greater or equal than {1}")]
        public int Sold { get; set; } = 0;

        /// <summary>
        /// The discount applied to the product. Only valid values are between 0 and 100.
        /// <br/>
        /// Giảm giá áp dụng cho sản phẩm. Chỉ có giá trị hợp lệ nằm giữa 0 và 100.
        /// </summary>
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "The {0} must have no more than 2 decimal places")]
        [Range(0, 100, ErrorMessage = "The {0} must be greater or equal than {1}")]
        public float Discount { get; set; } = 0;

        /// <summary>
        /// The status of the product (e.g., Inactive, Active, Block, Deleted). 
        /// <br/>
        /// Trạng thái của sản phẩm (ví dụ: Inactive, Active, Block, Deleted).
        /// </summary>
        public ProductStatus Status { get; set; } = ProductStatus.Inactive;

        /// <summary>
        /// The identifier of the user who owns or created the product.
        /// <br/>
        /// Định danh của người dùng sở hữu hoặc tạo ra sản phẩm.
        /// </summary>
        public required string UserName { get; set; }

        /// <summary>
        /// Calculates the price of the product after applying the discount.
        /// <br/>
        /// Tính toán giá của sản phẩm sau khi áp dụng giảm giá.
        /// </summary>
        /// <returns>
        /// The discounted price of the product.
        /// <br/>
        /// Giá sau khi đã giảm.
        /// </returns>
        public decimal GetPrice()
        {
            return Price - Price * (decimal)Discount / 100;
        }
    }

}