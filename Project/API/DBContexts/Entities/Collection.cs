namespace API.Data.Entities
{
    public class Collection
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public TypeCollection Type { get; set; }
        public StatusCollection Status { get; set; }
        public int Quantity { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
        public Product Product { get; set; }
        public Guid ProductId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } 
    }
}
