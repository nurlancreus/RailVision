using RailVision.Application.Abstractions;
using RailVision.Application.DTOs.Overpass;
using RailVision.Application.DTOs;
using Microsoft.Extensions.Logging;
using RailVision.Application.Abstractions.OverpassAPI;

namespace RailVision.Infrastructure.Services
{
    public class RailwayService(IRailwaysOverpassApiService overpassApiService, ILogger<RailwayService> logger) : IRailwayService
    {
        private readonly IRailwaysOverpassApiService _overpassApiService = overpassApiService;
        private readonly ILogger<RailwayService> _logger = logger;

        public async Task<OverpassResponseDTO> GetRailwaysDataAsync(CancellationToken cancellationToken = default)
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

            var railwayLines = railwaysData.Elements
                .Where(e => e.Type == "way" && e.Tags.TryGetValue("railway", out var value) && value == "rail")
                .Select(e =>
                {
                    var coordinates = e.Geometry.Select(geo => new CoordinateDTO
                    {
                        Latitude = geo.Lat,
                        Longitude = geo.Lon
                    });

                    return new RailwayLineDTO
                    {
                        ElementId = e.Id,
                        Coordinates = coordinates
                    };
                });

            _logger.LogInformation("Successfully processed railway lines.");
            return railwayLines;

        }
    }
}
