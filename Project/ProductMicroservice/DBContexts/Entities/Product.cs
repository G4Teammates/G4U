using MongoDB.Bson.Serialization.Attributes;
using ProductMicroservice.DBContexts.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductMicroservice.DBContexts.Entities
{
    #region SQL
    //public class Product
    //{
    //    public Guid Id { get; set; }
    //    public required string Name { get; set; }
    //    public string? Description { get; set; }
    //    [Column(TypeName = "decimal(18,2)")]
    //    public decimal Price { get; set; }
    //    public int Quantity { get; set; }
    //    public int NumberOfViews { get; set; }
    //    public int NumberOfPlays { get; set; }
    //    public int NumberOfLikes { get; set; }
    //    public float Discount { get; set; }
    //    public PlatformType Platform { get; set; }
    //    public ProductStatus Status { get; set; }

    //    public DateTime CreatedAt { get; set; }
    //    public DateTime UpdatedAt { get; set; }

    //    public Guid UserId { get; set; }

    //}
    #endregion

    #region noSQL
    /// <summary>
    /// Represents a product in the system.
    /// <br/>
    /// Đại diện cho một sản phẩm trong hệ thống.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Unique identifier for the product.
        /// <br/>
        /// Định danh duy nhất cho sản phẩm.
        /// </summary>
        [BsonId]
        [BsonElement("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the product.
        /// <br/>
        /// Tên của sản phẩm.
        /// </summary>
        [BsonElement("name")]
        public required string Name { get; set; }

        /// <summary>
        /// A description of the product.
        /// <br/>
        /// Mô tả về sản phẩm.
        /// </summary>
        [BsonElement("description")]
        public string? Description { get; set; }

        /// <summary>
        /// The price of the product.
        /// <br/>
        /// Giá của sản phẩm.
        /// </summary>
        [BsonElement("price")]
        public decimal Price { get; set; }

        /// <summary>
        /// The number of products sold.
        /// <br/>
        /// Số sản phẩm đã bán được.
        /// </summary>
        [BsonElement("sold")]
        public int Sold { get; set; }


        /// <summary>
        /// The number of views the product has received.
        /// <br/>
        /// Số lượt xem mà sản phẩm đã nhận được.
        /// </summary>
        [BsonElement("numberOfViews")]
        public int NumberOfViews { get; set; }

        /// <summary>
        /// The number of plays or interactions with the product.
        /// <br/>
        /// Số lượt chơi hoặc tương tác với sản phẩm.
        /// </summary>
        [BsonElement("numberOfPlays")]
        public int NumberOfPlays { get; set; }

        /// <summary>
        /// The number of likes the product has received.
        /// <br/>
        /// Số lượt thích mà sản phẩm đã nhận được.
        /// </summary>
        [BsonElement("numberOfLikes")]
        public int NumberOfLikes { get; set; }

        /// <summary>
        /// The discount applied to the product.
        /// <br/>
        /// Giảm giá áp dụng cho sản phẩm.
        /// </summary>
        [BsonElement("discount")]
        public float Discount { get; set; }

        /// <summary>
        /// The platform where the product is available (e.g., Window, Android, WebGL,...).
        /// <br/>
        /// Nền tảng nơi sản phẩm có sẵn (ví dụ: Window, Android, WebGL,...).
        /// </summary>
        [BsonElement("platform")]
        public PlatformType Platform { get; set; }

        /// <summary>
        /// The status of the product (e.g., Inactive, Active, Block, Deleted). 
        /// <br/>
        /// Trạng thái của sản phẩm (ví dụ: Inactive, Active, Block, Deleted).
        /// </summary>
        [BsonElement("status")]
        public ProductStatus Status { get; set; }

        /// <summary>
        /// The date and time when the product was created.
        /// <br/>
        /// Ngày và giờ khi sản phẩm được tạo ra.
        /// </summary>
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the product was last updated.
        /// <br/>
        /// Ngày và giờ khi sản phẩm được cập nhật lần cuối.
        /// </summary>
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// The identifier of the user who owns or created the product.
        /// <br/>
        /// Định danh của người dùng sở hữu hoặc tạo ra sản phẩm.
        /// </summary>
        [BsonElement("userId")]
        public Guid UserId { get; set; }
    }
    #endregion
}
