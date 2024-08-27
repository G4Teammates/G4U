namespace Category.DBContexts.Entities
{
    public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public CategoryType Type { get; set; }
        public string? Description { get; set; }
        public CategoryStatus Status { get; set; }
    }
}
