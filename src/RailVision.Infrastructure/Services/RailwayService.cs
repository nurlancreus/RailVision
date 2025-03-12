using RailVision.Application.Abstractions;
using RailVision.Application.DTOs.Overpass;
using RailVision.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace RailVision.Infrastructure.Services
{
    public class RailwayService(IOverpassApiService overpassApiService, ILogger<RailwayService> logger) : IRailwayService
    {
        private readonly IOverpassApiService _overpassApiService = overpassApiService;
        private readonly ILogger<RailwayService> _logger = logger;

        public async Task<OverpassResponseDTO> GetRailwayDataAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting railway data...");

            var result = await _overpassApiService.GetRailwaysDataAsync(cancellationToken);
            _logger.LogInformation("Successfully retrieved railway data.");
            return result;
        }

        public async Task<IEnumerable<RailwayLineDTO>> GetRailwayLinesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting railway lines...");

            var railwaysData = await _overpassApiService.GetRailwaysDataAsync(cancellationToken);

            _logger.LogInformation("Processing railway lines data...");

            var nodeDictionary = railwaysData.Elements
                .Where(e => e.Type == "node")
                .ToDictionary(e => e.Id, e => new { e.Lat, e.Lon });

            var railwayLines = railwaysData.Elements
                .Where(e => e.Type == "way" && e.Tags.TryGetValue("railway", out var value) && value == "rail")
                .Select(e =>
                {
                    var coordinates = e.Nodes
                        .Select(nodeId =>
                        {
                            nodeDictionary.TryGetValue(nodeId, out var node);
                            return new CoordinateDTO
                            {
                                Latitude = node?.Lat ?? 0,
                                Longitude = node?.Lon ?? 0
                            };
                        });

                    return new RailwayLineDTO
                    {
                        Id = e.Id,
                        Coordinates = coordinates
                    };
                });

            _logger.LogInformation("Successfully processed railway lines.");
            return railwayLines;

        }
    }
}
