using Microsoft.EntityFrameworkCore;
using StatisticalMicroservice.DBContexts.Entities;

namespace StatisticalMicroservice.DBContexts
{
    public class StatisticalDbcontext : DbContext
    {
        public StatisticalDbcontext(DbContextOptions<StatisticalDbcontext> options) : base(options)
        {
        }


        public DbSet<Statistical> Statistical { get; set; }
    }
}
