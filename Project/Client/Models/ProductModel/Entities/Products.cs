using Client.Models.ProductModel.Enum;

namespace Client.Models.ProductModel.Entities
{
    /// <summary>
    /// Represents a product in the system.
    /// <br/>
    /// Đại diện cho một sản phẩm trong hệ thống.
    /// </summary>
    public class Products
    {
        /// <summary>
        /// Unique identifier for the product.
        /// <br/>
        /// Định danh duy nhất cho sản phẩm.
        /// </summary>
        
        public required string Id { get; set; }

        /// <summary>
        /// The name of the product.
        /// <br/>
        /// Tên của sản phẩm.
        /// </summary>

        public required string Name { get; set; }

        /// <summary>
        /// A description of the product.
        /// <br/>
        /// Mô tả về sản phẩm.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The original price of the product before any discounts are applied.
        /// <br/>
        /// Giá gốc của sản phẩm trước khi áp dụng bất kỳ giảm giá nào.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The number of products sold.
        /// <br/>
        /// Số sản phẩm đã bán được.
        /// </summary>
        public int Sold { get; set; }

        /// <summary>
        /// The number of views <see cref="Interactions.NumberOfViews"/> and number of likes <see cref="Interactions.NumberOfLikes"/> the product has received. Default is 0.
        /// <br/>
        /// Số lượt xem <see cref="Interactions.NumberOfViews"/>  và lượt thích <see cref="Interactions.NumberOfLikes"/> mà sản phẩm nhận được. Mặc định là 0.
        /// </summary>
        public Interactions? Interactions { get; set; }

        /// <summary>
        /// The discount applied to the product. Only valid values are between 0 and 100.
        /// <br/>
        /// Giảm giá áp dụng cho sản phẩm. Chỉ có giá trị hợp lệ nằm giữa 0 và 100.
        /// </summary>
        public float Discount { get; set; }


        /// <summary>
        /// The platform where the product is available (e.g., Window, Android, WebGL,...).
        /// <br/>
        /// Nền tảng nơi sản phẩm có sẵn (ví dụ: Window, Android, WebGL,...).
        /// </summary>
        public PlatformType Platform { get; set; }

        /// <summary>
        /// A collection of related links associated with the product or content.
        /// <br/>
        /// Tập hợp các liên kết liên quan đến sản phẩm hoặc nội dung.
        /// </summary>
        public ICollection<Links>? Links { get; set; }


        /// <summary>
        /// A collection of categories that the product belongs to.
        /// <br/>
        /// Danh sách các danh mục mà sản phẩm thuộc về.
        /// </summary>
        public ICollection<Categories>? Categories { get; set; }


        /// <summary>
        /// The status of the product (e.g., Inactive, Active, Block, Deleted). 
        /// <br/>
        /// Trạng thái của sản phẩm (ví dụ: Inactive, Active, Block, Deleted).
        /// </summary>
        public ProductStatus Status { get; set; }



        /// <summary>
        /// The date and time when the product was created.
        /// <br/>
        /// Ngày và giờ khi sản phẩm được tạo ra.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the product was last updated.
        /// <br/>
        /// Ngày và giờ khi sản phẩm được cập nhật lần cuối.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// The identifier of the user who owns or created the product.
        /// <br/>
        /// Định danh của người dùng sở hữu hoặc tạo ra sản phẩm.

        /// </summary>

        public required string UserId { get; set; }
    }
}
