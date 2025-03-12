using RailVision.Application.Abstractions;
using RailVision.Application.DTOs.Overpass;
using RailVision.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace RailVision.Infrastructure.Services
{
    public class StationService(IOverpassApiService overpassApiService, ILogger<StationService> logger) : IStationService
    {
        private readonly IOverpassApiService _overpassApiService = overpassApiService;
        private readonly ILogger<StationService> _logger = logger;

        public async Task<IEnumerable<StationDTO>> GetStationCoordsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting station coordinates...");

            var stations = await GetStationsAsync(cancellationToken);

            var stationCoords = stations
                .Select(e => new StationDTO
                {
                    Id = e.Id,
                    Name = e.Tags.TryGetValue("name", out var name) ? name : "Unknown",
                    Coordinate = new CoordinateDTO
                    {
                        Latitude = e.Lat ?? 0,
                        Longitude = e.Lon ?? 0
                    }
                });

            _logger.LogInformation("Successfully retrieved station coordinates.");

            return stationCoords;
        }

        public async Task<IEnumerable<StationDTO>> GetStationNamesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting station names...");

            var stations = await GetStationsAsync(cancellationToken);

            var stationNames = stations.Select(s => new StationDTO
            {
                Id = s.Id,
                Name = s.Tags.TryGetValue("name", out var name) ? name : "Unknown"
            });

            _logger.LogInformation("Successfully retrieved station names.");

            return stationNames;
        }

        public async Task<OverpassResponseDTO> GetStationDataAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting station data...");

            var result = await _overpassApiService.GetStationsDataAsync(cancellationToken);
            _logger.LogInformation("Successfully retrieved station data.");

            return result;
        }

        private async Task<IEnumerable<ElementDTO>> GetStationsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting stations...");

            var stationsData = await _overpassApiService.GetStationsDataAsync(cancellationToken);

            var stations = stationsData.Elements
                .Where(e => (e.Type == "node" || e.Type == "way") &&
                            e.Tags.TryGetValue("railway", out var railwayType) &&
                            (railwayType == "station" || railwayType == "halt") /*&& !e.Tags.TryGetValue("subway", out var _)*/);

            _logger.LogInformation("Successfully retrieved station data from Overpass.");

            return stations;
        }
    }
}
