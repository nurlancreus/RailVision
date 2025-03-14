using Microsoft.Extensions.Logging;
using RailVision.Application.Abstractions;
using RailVision.Application.Abstractions.OverpassAPI;
using RailVision.Application.DTOs;
using RailVision.Application.DTOs.Overpass;

namespace RailVision.Infrastructure.Services
{
    public class TerrainService(ITerrainsOverpassApiService overpassApiService, ILogger<TerrainService> logger) : ITerrainService
    {
        private readonly ITerrainsOverpassApiService _overpassApiService = overpassApiService;
        private readonly ILogger<TerrainService> _logger = logger;

        public async Task<IEnumerable<ObstacleDTO>> GetTerrainObstaclesCoordsAsync(CancellationToken cancellationToken = default)
        {

            var result = new List<ObstacleDTO>();
            var coordinates = new List<CoordinateDTO>();

            var terrainObstaclesData = await _overpassApiService.GetNaturalTerrainObstaclesDataAsync(cancellationToken);

            foreach (var element in terrainObstaclesData.Elements)
            {
                var obstacle = new ObstacleDTO
                {
                    ElementId = element.Id
                };

                if (element.Type == "node")
                {
                    obstacle.Coordinate = new CoordinateDTO
                    {
                        Latitude = element.Lat ?? 0,
                        Longitude = element.Lon ?? 0
                    };
                }
                else if (element.Type == "way")
                {
                    coordinates.AddRange(element.Geometry.Select(geo => new CoordinateDTO
                    {
                        Latitude = geo.Lat,
                        Longitude = geo.Lon
                    }));
                }

                if (element.Tags.TryGetValue("natural", out var naturalType))
                {
                    obstacle.Type = naturalType;
                }
                else if (element.Tags.TryGetValue("landuse", out var landuseType))
                {
                    obstacle.Type = landuseType;
                }
                else if (element.Tags.TryGetValue("barries", out var barrierType))
                {
                    obstacle.Type = barrierType;
                }
                else
                {
                    obstacle.Type = "Unknown";
                }

                if (element.Tags.TryGetValue("name", out var name))
                {
                    obstacle.Name = name;
                }

                obstacle.Coordinates = coordinates;
                result.Add(obstacle);
            }

            return result;
        }

        public async Task<OverpassResponseDTO> GetNaturalTerrainObstaclesDataAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting natural terrain obstacles data...");

            var result = await _overpassApiService.GetNaturalTerrainObstaclesDataAsync(cancellationToken);

            _logger.LogInformation("Successfully retrieved natural terrain obstacles data.");
            return result;
        }

        public async Task<OverpassResponseDTO> GetManMadeTerrainObstaclesDataAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting man-made terrain obstacles data...");

            var result = await _overpassApiService.GetManMadeTerrainObstaclesDataAsync(cancellationToken);

            _logger.LogInformation("Successfully retrieved man-made terrain obstacles data.");
            return result;
        }

        public Task<OverpassResponseDTO> GetTerrainObstaclesDataAsync(string type = "", CancellationToken cancellationToken = default)
        {
            return type switch
            {
                "natural" => GetNaturalTerrainObstaclesDataAsync(cancellationToken),
                "man-made" => GetManMadeTerrainObstaclesDataAsync(cancellationToken),
                "" => _overpassApiService.GetTerrainObstaclesDataAsync(cancellationToken),
                _ => throw new ArgumentException("Invalid terrain type")
            };
        }
    }
}
