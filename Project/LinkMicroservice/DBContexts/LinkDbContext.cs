
using LinkMicroservice.DBContexts.Entities;
using Microsoft.EntityFrameworkCore;

namespace LinkMicroservice.DBContexts
{
    public class LinkDbContext : DbContext
    {
        public LinkDbContext(DbContextOptions<LinkDbContext> options) : base(options)
        {
        }

        public DbSet<Link> Links { get; set; }
    }
}
