using Collection.DBContexts.Enum;

namespace Collection.DBContexts.Entities
{
    public class Collection
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public CollectionType Type { get; set; }
        public CollectionStatus Status { get; set; }
        public int Quantity { get; set; }

        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }
}
