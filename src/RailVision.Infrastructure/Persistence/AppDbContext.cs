using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace RailVision.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        // add-migration init -OutputDir Persistence/Migrations
        protected override void OnModelCreating(ModelBuilder builder)
        {
           // builder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(typeof(ApplicationUserConfiguration))!);

            base.OnModelCreating(builder);
        }
    }
}
