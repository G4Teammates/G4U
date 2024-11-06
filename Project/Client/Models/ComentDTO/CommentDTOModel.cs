using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Client.Models.ComentDTO
{
    public class CommentDTOModel
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
        [Required(ErrorMessage = "Comment content is required.")]
        [StringLength(500, ErrorMessage = "Comment content must not exceed {1} characters.")]
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

        [BsonElement("userName")]
        public string UserName { get; set; }

        /// <summary>
        /// The identifier of the product related to the comment.
        /// <br/>
        /// ID của sản phẩm liên quan đến bình luận.
        /// </summary>
        [BsonElement("productId")]
        [Required(ErrorMessage = "Product ID is required.")]
        public string ProductId { get; set; }

        /// <summary>
        /// The identifier of the parent comment, if any (for nested comments).
        /// <br/>
        /// ID của bình luận cha, nếu có (dành cho bình luận lồng nhau).
        /// </summary>
        [BsonElement("parentId")]
        public string? ParentId { get; set; }

        /// <summary>
        /// The date and time when the comment was created.
        /// <br/>
        /// Ngày và giờ khi bình luận được tạo ra.
        /// </summary>
        [BsonElement("createdAt")]
        [Required(ErrorMessage = "Creation time of the comment is required.")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the comment was last updated.
        /// <br/>
        /// Ngày và giờ khi bình luận được cập nhật lần cuối.
        /// </summary>
        [BsonElement("updatedAt")]
        [Required(ErrorMessage = "Update time of the comment is required.")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// The number of dislikes the comment has received.
        /// <br/>
        /// Số lượt không thích mà bình luận đã nhận được.
        /// </summary>
        [BsonElement("numberOfDisLikes")]
        public int NumberOfDisLikes { get; set; }
    }
}
