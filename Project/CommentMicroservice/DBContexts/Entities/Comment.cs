using Comment.DBContexts.Enum;

namespace Comment.DBContexts.Entities
{
    public class Comment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Content { get; set; }
        public int NumberOfLikes { get; set; }
        public CommentStatus Status { get; set; }

        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public Comment? Parent { get; set; }
        public Guid ParentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }
}
