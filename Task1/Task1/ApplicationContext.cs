using Microsoft.EntityFrameworkCore;

namespace Task1
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Entity> table => Set<Entity>();
        public ApplicationContext() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=tableDb;Username=postgres;Password=29kurlwg");
        }
    }
}
