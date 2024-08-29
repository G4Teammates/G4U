
using CollectionMicroservice.DBContexts.Entities;
using Microsoft.EntityFrameworkCore;

namespace CollectionMicroservice.DBContexts
{
    public class CollectionDbContext : DbContext
    {
        public CollectionDbContext(DbContextOptions<CollectionDbContext> options) : base(options)
        {
        }

        public DbSet<Collection> Collections { get; set; }
    }
}
