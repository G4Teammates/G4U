using Microsoft.EntityFrameworkCore;

namespace CommentMicroservice.DBContexts
{
    public class ApplicationDbContext 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }


    }
}
