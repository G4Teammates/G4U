namespace API.Data.Entities
{
    public class Comment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Content { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
        public Product Product { get; set; }
        public Guid ProductId { get; set; }
        public Comment Parent { get; set; }
        public Guid ParentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }
}
