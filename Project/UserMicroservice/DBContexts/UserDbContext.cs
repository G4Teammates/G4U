using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserMicroservice.DBContexts.Entities;

namespace UserMicroservice.DBContexts
{
    public class UserDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }


    }
}
