using CommentMicroservice.DBContexts.Enum;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Client.Models.ComentDTO
{
    public class CreateCommentDTOModel
    {
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
        [Required(ErrorMessage = "User ID is required.")]
        public string UserId { get; set; }

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

    }
}
