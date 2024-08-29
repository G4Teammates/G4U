
using Microsoft.EntityFrameworkCore;
using ProductMicroservice.DBContexts.Entities;

namespace ProductMicroservice.DBContexts
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}
