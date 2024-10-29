using ProductMicroservice.DBContexts.Enum;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProductMicroservice.Models
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
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        /// <summary>
        /// The name of the product.
        /// <br/>
        /// Tên của sản phẩm.
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public required string Name { get; set; }

        /// <summary>
        /// A description of the product.
        /// <br/>
        /// Mô tả về sản phẩm.
        /// </summary>
        [MaxLength(10000, ErrorMessage = "The {0} must be at max {1} characters long.")]
        public string? Description { get; set; }

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
        /// The number of views and likes the product has received. Default is 0.
        /// <br/>
        /// Số lượt xem và lượt thích mà sản phẩm nhận được. Mặc định là 0.
        /// </summary>
        public InteractionModel? Interactions { get; set; } = new InteractionModel { NumberOfLikes = 0, NumberOfViews = 0 };

        /// <summary>
        /// The discount applied to the product. Only valid values are between 0 and 100.
        /// <br/>
        /// Giảm giá áp dụng cho sản phẩm. Chỉ có giá trị hợp lệ nằm giữa 0 và 100.
        /// </summary>
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "The {0} must have no more than 2 decimal places")]
        [Range(0, 100, ErrorMessage = "The {0} must be greater or equal than {1}")]
        public float Discount { get; set; } = 0;

        /// <summary>
        /// The platform where the product is available (e.g., Window, Android, WebGL,...).
        /// <br/>
        /// Nền tảng nơi sản phẩm có sẵn (ví dụ: Window, Android, WebGL,...).
        /// </summary>
        public PlatformType Platform { get; set; } = PlatformType.Unknown;

        /// <summary>
        /// Represents a Link entity in the system. Each link is associated with a user and can optionally be tied to a product.<br/>
        /// Đại diện cho một thực thể liên kết trong hệ thống. Mỗi liên kết được gắn với một người dùng và có thể được liên kết với một sản phẩm.
        /// </summary>
        public ICollection<LinkModel>? Links { get; set; }

        /// <summary>
        /// A collection of categories that the product belongs to.
        /// <br/>
        /// Danh sách các danh mục mà sản phẩm thuộc về.
        /// </summary>
        public ICollection<CategoryModel>? Categories { get; set; }

        /// <summary>
        /// The status of the product (e.g., Inactive, Active, Block, Deleted). 
        /// <br/>
        /// Trạng thái của sản phẩm (ví dụ: Inactive, Active, Block, Deleted).
        /// </summary>
        public ProductStatus Status { get; set; } = ProductStatus.Inactive;

        /// <summary>
        /// The date and time when the product was created.
        /// <br/>
        /// Ngày và giờ khi sản phẩm được tạo ra.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The date and time when the product was last updated.
        /// <br/>
        /// Ngày và giờ khi sản phẩm được cập nhật lần cuối.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

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
            return Price - (Price * (decimal)Discount / 100);
        }
    }

}