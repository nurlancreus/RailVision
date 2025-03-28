using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RailVision.Domain.Entities;
using RailVision.Domain.Entities.Coordinates;
using RailVision.Infrastructure.Persistence.Configurations;
using System.Reflection;

namespace RailVision.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        // add-migration init -OutputDir Persistence/Migrations
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(typeof(CoordinateConfigurations))!);

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

        public DbSet<PopulationCenter> PopulationCenters { get; set; }
        public DbSet<Railway> Railways { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<Obstacle> Obstacles { get; set; }
        public DbSet<Coordinate> Coordinates { get; set; }
        public DbSet<RailwayCoordinate> RailwayCoordinates { get; set; }
        public DbSet<StationCoordinate> StationCoordinates { get; set; }
        public DbSet<ObstacleCoordinate> ObstacleCoordinates { get; set; }

    }
}
