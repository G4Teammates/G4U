using CommentMicroservice.DBContexts.Enum;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace CommentMicroservice.DBContexts.Entities
{
    public class Comment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required(ErrorMessage = "Content is required")]
        [MaxLength(10000, ErrorMessage ="Content must be less than 10,000 characters")]
        public required string Content { get; set; }
        public int NumberOfLikes { get; set; }
        public CommentStatus Status { get; set; }

        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }

        [DeleteBehavior(DeleteBehavior.NoAction)]
        public Comment? Parent { get; set; }
        public Guid? ParentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }

}
