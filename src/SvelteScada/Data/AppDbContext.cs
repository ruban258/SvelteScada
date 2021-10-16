using Microsoft.EntityFrameworkCore;
using GQLServer.Models;

namespace GQLServer.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
            
        }
        public DbSet<Recepie> Recepies { get; set; }
    }
}