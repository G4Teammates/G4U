using LinkMicroservice.DBContexts.Enum;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace LinkMicroservice.DBContexts.Entities
{
    #region SQL
    //public class Link
    //{
    //    public Guid Id { get; set; } = Guid.NewGuid();
    //    public required string ProviderName { get; set; }
    //    public required string Url { get; set; }
    //    public LinkType Type { get; set; }
    //    public LinkStatus Status { get; set; }

    //    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    //    public DateTime UpdateAt { get; set; }

    //    public Guid ProductId { get; set; }
    //}
    #endregion

    #region noSQL
    /// <summary>
    /// Represents a Link entity in the system. Each link is associated with a user and can optionally be tied to a product.<br/>
    /// Đại diện cho một thực thể liên kết trong hệ thống. Mỗi liên kết được gắn với một người dùng và có thể được liên kết với một sản phẩm.
    /// </summary>
    public class Link
    {
        /// <summary>
        /// Unique identifier for the link.<br/>
        /// Định danh duy nhất cho liên kết.
        /// </summary>
        [BsonId]
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the link provider (social media or other platforms such as Facebook, Google Drive, TerraBox, GitHub, etc.).<br/>
        /// Tên của nhà cung cấp liên kết (mạng xã hội hoặc các nền tảng khác như Facebook, Google Drive, TerraBox, GitHub, v.v.).
        /// </summary>
        [BsonElement("providerName")]
        public required string ProviderName { get; set; }

        /// <summary>
        /// The URL of the link.<br/>
        /// URL của liên kết.
        /// </summary>
        [BsonElement("url")]
        public required string Url { get; set; }

        /// <summary>
        /// The type of the link, which could represent various categories (e.g., social or product-related).<br/>
        /// Loại của liên kết, có thể đại diện cho các danh mục khác nhau (ví dụ: liên quan đến mạng xã hội hoặc sản phẩm).
        /// </summary>
        [BsonElement("type")]
        public LinkType Type { get; set; }

        /// <summary>
        /// The status of the link (Active, Inactive, Block, Deleted).<br/>
        /// Trạng thái của liên kết (Kích hoạt, Không kích hoạt, Bị chặn, Đã xóa).
        /// </summary>
        [BsonElement("status")]
        public LinkStatus Status { get; set; }

        /// <summary>
        /// The creation date of the link.<br/>
        /// Ngày tạo liên kết.
        /// </summary>
        [BsonElement("createAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// The date when the link was last updated.<br/>
        /// Ngày liên kết được cập nhật lần cuối.
        /// </summary>
        [BsonElement("updateAt")]
        public DateTime UpdateAt { get; set; }

        /// <summary>
        /// The identifier of the product associated with the link, if applicable.<br/>
        /// Định danh của sản phẩm liên kết với liên kết, nếu có.
        /// </summary>
        [BsonElement("productId")]
        public Guid ProductId { get; set; }

        /// <summary>
        /// The identifier of the user who owns the link. <br/>
        /// Định danh của người dùng sở hữu liên kết.
        /// </summary>
        [BsonElement("userId")]
        public Guid UserId { get; set; }
    }
    #endregion

}
