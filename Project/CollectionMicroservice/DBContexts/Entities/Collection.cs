using CollectionMicroservice.DBContexts.Enum;
using MongoDB.Bson.Serialization.Attributes;

namespace CollectionMicroservice.DBContexts.Entities
{
    #region SQL
    //public class Collection
    //{
    //    public Guid Id { get; set; }
    //    public required string Name { get; set; }
    //    public CollectionType Type { get; set; }
    //    public CollectionStatus Status { get; set; }
    //    public int Quantity { get; set; }

    //    public Guid UserId { get; set; }
    //    public Guid ProductId { get; set; }

    //    public DateTime CreatedAt { get; set; }
    //    public DateTime UpdatedAt { get; set; }
    //}
    #endregion

    #region noSQL
    /// <summary>
    /// Represents a collection in the system.
    /// <br/>
    /// Đại diện cho một bộ sưu tập trong hệ thống.
    /// </summary>
    public class Collection
    {
        /// <summary>
        /// Unique identifier for the collection.
        /// <br/> 
        /// Định danh duy nhất cho bộ sưu tập.
        /// </summary>
        [BsonId]
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the collection.
        /// <br/> 
        /// Tên của bộ sưu tập.
        /// </summary>
        [BsonElement("name")]
        public required string Name { get; set; }

        /// <summary>
        /// The type of the collection.
        /// <br/> 
        /// Loại của bộ sưu tập.
        /// </summary>
        [BsonElement("type")]
        public CollectionType Type { get; set; }

        /// <summary>
        /// The status of the collection.
        /// <br/> 
        /// Trạng thái của bộ sưu tập.
        /// </summary>
        [BsonElement("status")]
        public CollectionStatus Status { get; set; }

        /// <summary>
        /// The quantity of items in the collection.
        /// <br/> 
        /// Số lượng các mục trong bộ sưu tập.
        /// </summary>
        [BsonElement("quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// The identifier of the user who owns the collection.
        /// <br/> 
        /// Định danh của người dùng sở hữu bộ sưu tập.
        /// </summary>
        [BsonElement("userId")]
        public Guid UserId { get; set; }

        /// <summary>
        /// The identifier of the product associated with the collection, if applicable.
        /// <br/> 
        /// Định danh của sản phẩm liên kết với bộ sưu tập, nếu có.
        /// </summary>
        [BsonElement("productId")]
        public Guid ProductId { get; set; }

        /// <summary>
        /// The creation date of the collection.
        /// <br/> 
        /// Ngày tạo bộ sưu tập.
        /// </summary>
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The date when the collection was last updated.
        /// <br/> 
        /// Ngày bộ sưu tập được cập nhật lần cuối.
        /// </summary>
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
    #endregion

}
