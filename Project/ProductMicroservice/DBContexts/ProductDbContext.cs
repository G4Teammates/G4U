
using Microsoft.EntityFrameworkCore;
using ProductMicroservice.DBContexts.Entities;

namespace ProductMicroservice.DBContexts
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
        {
        }


        public DbSet<Products> Products { get; set; }

        /*public DbSet<Categories> Categories { get; set; }
        public DbSet<Interactions> Interactions { get; set; }*/

    }
}
