using InvestigationClearance.Models.Domain;
using Microsoft.EntityFrameworkCore;
using InvestigationClearance.Models;

namespace InvestigationClearance.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Clearance> Clearances { get; set; }

        public DbSet<InvestigationClearance.Models.ClearanceViewModel>? ClearanceViewModel { get; set; }
    }
}
