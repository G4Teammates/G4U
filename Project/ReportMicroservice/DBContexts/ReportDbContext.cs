using Microsoft.EntityFrameworkCore;
using ReportMicroservice.DBContexts.Entities;

namespace ReportMicroservice.DBContexts
{
    public class ReportDbContext : DbContext 
    {
        public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options)
        {
        }
        public DbSet<Reports> reports { get; set; }
    }
}
