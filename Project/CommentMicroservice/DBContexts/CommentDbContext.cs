
using CommentMicroservice.DBContexts.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommentMicroservice.DBContexts
{
    public class CommentDbContext : DbContext
    {
        public CommentDbContext(DbContextOptions<CommentDbContext> options) : base(options)
        {
        }

        public DbSet<Comment> Comments { get; set; }
    }
}
