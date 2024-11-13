using CommentMicroservice.DBContexts.Entities;
using CommentMicroservice.DBContexts.Enum;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CommentMicroservice.Models
{
    public class CommentModel
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
        [BsonElement("userName")]
        public string UserName { get; set; }

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

        [BsonElement("userDisLike")]
        public ICollection<UserDisLikesModel>? UserDisLikes { get; set; } = null;

        [BsonElement("userLike")]
        public ICollection<UserLikesModel>? UserLikes { get; set; } =null;
    }
}
