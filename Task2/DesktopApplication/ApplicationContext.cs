using Microsoft.EntityFrameworkCore;

namespace DesktopApplication
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=osvDB;Username=postgres;Password=29kurlwg");
        }
    }
}
