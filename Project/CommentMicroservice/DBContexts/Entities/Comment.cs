using CommentMicroservice.DBContexts.Enum;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace CommentMicroservice.DBContexts.Entities
{
    #region SQL
    //public class Comment
    //{
    //    public Guid Id { get; set; } = Guid.NewGuid();
    //    [Required(ErrorMessage = "Content is required")]
    //    [MaxLength(10000, ErrorMessage ="Content must be less than 10,000 characters")]
    //    public required string Content { get; set; }
    //    public int NumberOfLikes { get; set; }
    //    public CommentStatus Status { get; set; }

    //    public Guid UserId { get; set; }
    //    public Guid ProductId { get; set; }

    //    [DeleteBehavior(DeleteBehavior.NoAction)]
    //    public Comment? Parent { get; set; }
    //    public Guid? ParentId { get; set; }

    //    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    //    public DateTime UpdatedAt { get; set; }
    //}
    #endregion

    #region noSQL
    /// <summary>
    /// Represents a comment in the system. Each comment can be associated with a product and a user.
    /// <br/>
    /// Đại diện cho một bình luận trong hệ thống. Mỗi bình luận có thể liên kết với một sản phẩm và một người dùng.
    /// </summary>
    public class Comment
    {
        /// <summary>
        /// Unique identifier for the comment.
        /// <br/>
        /// ID của bình luận.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        /// <summary>
        /// The content of the comment.
        /// <br/>
        /// Nội dung của bình luận.
        /// </summary>
        [BsonElement("content")]
        public required string Content { get; set; }

        /// <summary>
        /// The number of likes the comment has received.
        /// <br/>
        /// Số lượt thích mà bình luận đã nhận được.
        /// </summary>
        [BsonElement("numberOfLikes")]
        public int NumberOfLikes { get; set; }

        /// <summary>
        /// The status of the comment (e.g., Hidden, Visible).
        /// <br/>
        /// Trạng thái của bình luận (ví dụ: Ẩn, Hiển thị).
        /// </summary>
        [BsonElement("status")]
        public CommentStatus Status { get; set; }

        /// <summary>
        /// The identifier of the user who made the comment.
        /// <br/>
        /// ID của người dùng đã đăng bình luận.
        /// </summary>
        [BsonElement("userId")]
        public string UserId { get; set; }

        /// <summary>
        /// The identifier of the product related to the comment.
        /// <br/>
        /// ID của sản phẩm liên quan đến bình luận.
        /// </summary>
        [BsonElement("productId")]
        public string ProductId { get; set; }

        /// <summary>
        /// The identifier of the parent comment, if any (for nested comments).
        /// <br/>
        /// ID của bình luận cha, nếu có (dành cho bình luận lồng nhau).
        /// </summary>
        [BsonElement("parentId")]
        public string? ParentId { get; set; }

        /// <summary>
        /// A list of replies to this comment, if applicable.
        /// <br/>
        /// Danh sách các bình luận trả lời bình luận này, nếu có.
        /// </summary>    
        /*[BsonIgnore]
        //[BsonElement("replies")]
        public List<Comment>? Replies { get; set; }*/

        /// <summary>
        /// The date and time when the comment was created.
        /// <br/>
        /// Ngày và giờ khi bình luận được tạo ra.
        /// </summary>
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the comment was last updated.
        /// <br/>
        /// Ngày và giờ khi bình luận được cập nhật lần cuối.
        /// </summary>
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// The number of likes the comment has received.
        /// <br/>
        /// Số lượt thích mà bình luận đã nhận được.
        /// </summary>
        [BsonElement("numberOfDisLikes")]
        public int NumberOfDisLikes { get; set; }
    }
    #endregion


}
