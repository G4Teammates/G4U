using Microsoft.EntityFrameworkCore;
using CategoryMicroservice.DBContexts.Entities;

namespace CategoryMicroservice.DBContexts
{
    public class CategoryDbContext : DbContext
    {
        public CategoryDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<CategoryDetail> CategoryDetails { get; set; }
    }
}
