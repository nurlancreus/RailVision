using Microsoft.Extensions.Logging;
using RailVision.Application.Abstractions;
using RailVision.Application.Abstractions.OverpassAPI;
using RailVision.Application.DTOs.Overpass;

namespace RailVision.Infrastructure.Services
{
    public class TerrainService(ITerrainsOverpassApiService overpassApiService, ILogger<TerrainService> logger) : ITerrainService
    {
        private readonly ITerrainsOverpassApiService _overpassApiService = overpassApiService;
        private readonly ILogger<TerrainService> _logger = logger;

        public Task<OverpassResponseDTO> GetTerrainObstaclesCoordsAsync(CancellationToken cancellationToken = default)
        {

            throw new NotImplementedException();
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

        public Task<OverpassResponseDTO> GetTerrainObstaclesDataAsync(string type = "natural", CancellationToken cancellationToken = default)
        {
            return type switch
            {
                "natural" => GetNaturalTerrainObstaclesDataAsync(cancellationToken),
                "man-made" => GetManMadeTerrainObstaclesDataAsync(cancellationToken),
                _ => throw new ArgumentException("Invalid terrain type")
            };
        }
    }
}
