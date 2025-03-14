using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RailVision.Application.Abstractions.OverpassAPI;
using RailVision.Domain.Entities;
using RailVision.Domain.Entities.Coordinates;

namespace RailVision.Infrastructure.Persistence
{
    public static class OverpassDataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();

            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(typeof(OverpassDataSeeder));
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var isDataBaseAvailable = AppDbContext.CheckDatabaseAvailability(configuration);
            if (!isDataBaseAvailable)
            {
                logger.LogInformation("Database is not available, skipping Overpass Data Seeder...");
                return;
            }

            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var overpassService = scope.ServiceProvider.GetRequiredService<IOverpassApiService>();

            logger.LogInformation("Starting Overpass Data Seeder...");

            using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await SeedRailwaysAsync(dbContext, overpassService, logger, cancellationToken);
                await SeedStationsAsync(dbContext, overpassService, logger, cancellationToken);
                await SeedObstaclesAsync(dbContext, overpassService, logger, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                logger.LogInformation("Overpass Data Seeder finished successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during seeding. Rolling back...");
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private static async Task SeedRailwaysAsync(AppDbContext dbContext, IOverpassApiService overpassService, ILogger logger, CancellationToken cancellationToken)
        {
            if (await dbContext.Railways.AnyAsync(cancellationToken))
            {
                logger.LogInformation("Railways data already seeded, skipping...");
                return;
            }

            logger.LogInformation("Seeding Railways data...");
            var railwaysData = await overpassService.GetRailwaysDataAsync(cancellationToken);

            var railways = railwaysData.Elements
                .Where(e => e.Type == "way" && e.Tags.TryGetValue("railway", out var value) && value == "rail");

            var railwaysEntities = railways.Select(e => new Railway
            {
                Id = Guid.NewGuid(),
                ElementId = e.Id
            })
            .ToList();

            // Insert Railways first
            await dbContext.BulkInsertAsync(railwaysEntities, new BulkConfig
            {
                BatchSize = 500,
                PreserveInsertOrder = true
            }, cancellationToken: cancellationToken);

            // Insert RailwayCoordinates
            var railwayCoordinates = railways
                .SelectMany(e =>
                {
                    var railway = railwaysEntities.First(r => r.ElementId == e.Id);
                    return e.Geometry.Select(geo => new RailwayCoordinate
                    {
                        Id = Guid.NewGuid(),
                        RailwayId = railway.Id,
                        Latitude = geo.Lat,
                        Longitude = geo.Lon
                    });
                })
                .ToList();

            await dbContext.BulkInsertAsync(railwayCoordinates, new BulkConfig
            {
                BatchSize = 500,
                PreserveInsertOrder = true
            }, cancellationToken: cancellationToken);

            logger.LogInformation("Railways seeding completed.");
        }

        private static async Task SeedStationsAsync(AppDbContext dbContext, IOverpassApiService overpassService, ILogger logger, CancellationToken cancellationToken)
        {
            if (await dbContext.Stations.AnyAsync(cancellationToken))
            {
                logger.LogInformation("Stations data already seeded, skipping...");
                return;
            }

            logger.LogInformation("Seeding Stations data...");
            var stationsData = await overpassService.GetStationsDataAsync(cancellationToken);

            var stations = stationsData.Elements
                .Where(e => (e.Type == "node" || e.Type == "way") &&
                            e.Tags.TryGetValue("railway", out var railwayType) &&
                            (railwayType == "station" || railwayType == "halt") &&
                            !e.Tags.ContainsKey("subway"));

            var stationEntities = stations.Select(e => new Station
            {
                Id = Guid.NewGuid(),
                ElementId = e.Id,
                Name = e.Tags.TryGetValue("name", out string? value) ? value : "Unknown"
            })
                .ToList();

            // Insert Stations first
            await dbContext.BulkInsertAsync(stationEntities, new BulkConfig
            {
                BatchSize = 500,
                PreserveInsertOrder = true
            }, cancellationToken: cancellationToken);

            // Insert StationCoordinates
            var stationCoordinates = stations
                .Select(e =>
                {
                    var station = stationEntities.First(s => s.ElementId == e.Id);
                    return new StationCoordinate
                    {
                        Id = Guid.NewGuid(),
                        StationId = station.Id,
                        Latitude = e.Lat ?? 0,
                        Longitude = e.Lon ?? 0
                    };
                })
                .ToList();

            await dbContext.BulkInsertAsync(stationCoordinates, new BulkConfig
            {
                BatchSize = 500,
                PreserveInsertOrder = true
            }, cancellationToken: cancellationToken);

            logger.LogInformation("Stations seeding completed.");
        }

        private static async Task SeedObstaclesAsync(AppDbContext dbContext, IOverpassApiService overpassService, ILogger logger, CancellationToken cancellationToken)
        {
            if (await dbContext.Obstacles.AnyAsync(cancellationToken))
            {
                logger.LogInformation("Terrain Obstacles data already seeded, skipping...");
                return;
            }

            logger.LogInformation("Seeding Terrain Obstacles data...");
            var terrainObstaclesData = await overpassService.GetNaturalTerrainObstaclesDataAsync(cancellationToken);

            var obstacleEntities = terrainObstaclesData.Elements
                .Select(e => new Obstacle
                {
                    Id = Guid.NewGuid(),
                    ElementId = e.Id,
                    Type = e.Tags.ContainsKey("natural") ? e.Tags["natural"] :
                           e.Tags.ContainsKey("landuse") ? e.Tags["landuse"] :
                           e.Tags.ContainsKey("barrier") ? e.Tags["barrier"] : "Unknown",
                    Name = e.Tags.ContainsKey("name") ? e.Tags["name"] : "Unknown"
                })
                .ToList();

            // Insert Obstacles first
            await dbContext.BulkInsertAsync(obstacleEntities, new BulkConfig
            {
                BatchSize = 500,
                PreserveInsertOrder = true
            }, cancellationToken: cancellationToken);

            // Insert ObstacleCoordinates
            var obstacleCoordinates = terrainObstaclesData.Elements
                .SelectMany(e =>
                {
                    var obstacle = obstacleEntities.First(o => o.ElementId == e.Id);
                    if (e.Type == "node")
                    {
                        return
                        [
                            new ObstacleCoordinate
                            {
                                Id = Guid.NewGuid(),
                                ObstacleId = obstacle.Id,
                                Latitude = e.Lat ?? 0,
                                Longitude = e.Lon ?? 0
                            }
                        ];
                    }
                    else if (e.Type == "way")
                    {
                        return e.Geometry.Select(geo => new ObstacleCoordinate
                        {
                            Id = Guid.NewGuid(),
                            ObstacleId = obstacle.Id,
                            Latitude = geo.Lat,
                            Longitude = geo.Lon
                        }).ToList();
                    }
                    return [];
                })
                .ToList();

            await dbContext.BulkInsertAsync(obstacleCoordinates, new BulkConfig
            {
                BatchSize = 500,
                PreserveInsertOrder = true
            }, cancellationToken: cancellationToken);

            logger.LogInformation("Terrain Obstacles seeding completed.");
        }
    }
}
