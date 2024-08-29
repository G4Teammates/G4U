using Microsoft.EntityFrameworkCore;
using OrderMicroservice.DBContexts.Entities;

namespace OrderMicroservice.DBContexts
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }

    }
}
