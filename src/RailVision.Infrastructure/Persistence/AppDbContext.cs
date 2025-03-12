using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        public static bool CheckDatabaseAvailability(IConfiguration configuration)
        {
            try
            {
                using var scope = new ServiceCollection()
                    .AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(configuration.GetConnectionString("Default")))
                    .BuildServiceProvider()
                    .CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return dbContext.Database.CanConnect();
            }
            catch
            {
                return false;
            }
        }
    }
}
