namespace API.Data.Entities
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 0
        public int NumberOfViews { get; set; } = 0;
        public int NumberOfPlays { get; set; } = 0;
        public int NumberOfLikes { get; set; } = 0;
        public TypePlatform Platform { get; set; }
        public StatusProduct Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; }
        public Guid UserId { get; set; }
    }
}
