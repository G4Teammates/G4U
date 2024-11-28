using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;
using System.Text.RegularExpressions;
using UserMicroservice.DBContexts.Entities;

namespace UserMicroservice.DBContexts
{
    public class UserDbContext : DbContext
    {

        public DbSet<User> Users { get; init; }


        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().ToCollection("Users");

        }
    }
}
