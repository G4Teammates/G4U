using OrderMicroservice.DBContexts.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderMicroservice.DBContexts.Entities
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public required string PaymentMethod { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Collection Table
        public Guid CartId { get; set; }
        //Product Table
        public Guid ProductId { get; set; }

    }
}
