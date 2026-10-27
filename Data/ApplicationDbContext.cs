
using Microsoft.EntityFrameworkCore;
using Pawhaven.Models;

namespace Pawhaven.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Pet> Pets { get; set; } = null!;
    }
}