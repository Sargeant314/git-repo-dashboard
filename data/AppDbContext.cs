using Microsoft.EntityFrameworkCore;
using RepoDashboard.Models;

namespace RepoDashboard.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ProjectNote> Projects {get; set;} 
    }
}