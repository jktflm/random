using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Data
{
    public class ClearancesAPIDbContext : DbContext
    {
        public ClearancesAPIDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Clearance> Clearances { get; set; }
    }
}
