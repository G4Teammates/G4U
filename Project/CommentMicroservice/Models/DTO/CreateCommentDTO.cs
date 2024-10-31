using CommentMicroservice.DBContexts.Enum;

namespace CommentMicroservice.Models.DTO
{
    public class CreateCommentDTO
    {
        public required string Content { get; set; }
        public CommentStatus Status { get; set; }
        public string UserId { get; set; }
        public string ProductId { get; set; }
        public string? ParentId { get; set; }

    }
}
