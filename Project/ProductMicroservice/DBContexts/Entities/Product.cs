using Product.DBContexts.Enum;

namespace Product.DBContexts.Entities
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int NumberOfViews { get; set; }
        public int NumberOfPlays { get; set; }
        public int NumberOfLikes { get; set; }
        public PlatformType Platform { get; set; }
        public ProductStatus Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }

        public Guid UserId { get; set; }

    }
}
