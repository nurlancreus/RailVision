using RailVision.Application.Abstractions;
using RailVision.Application.DTOs.Overpass;
using RailVision.Application.DTOs;
using Microsoft.Extensions.Logging;
using RailVision.Application.Abstractions.OverpassAPI;
using RailVision.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RailVision.Application.Abstractions.Cache;
using RailVision.Domain.Entities;
using RailVision.Application.Helpers;

namespace RailVision.Infrastructure.Services
{
    public class StationService(IStationsOverpassApiService overpassApiService, AppDbContext dbContext, ICacheManagement cacheManager, ILogger<StationService> logger) : IStationService
    {
        private readonly IStationsOverpassApiService _overpassApiService = overpassApiService;
        private readonly AppDbContext _dbContext = dbContext;
        private readonly ICacheManagement _cacheManager = cacheManager;
        private readonly ILogger<StationService> _logger = logger;
        private readonly TimeSpan AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);

        public async Task<IEnumerable<StationDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var cacheKey = "GetAllStations";

            var cachedStations = await _cacheManager.GetCachedDataByKeyAsync<List<Station>>(cacheKey, cancellationToken);

            if (cachedStations != null)
            {
                _logger.LogInformation("Retrieved stations from cache.");
                return cachedStations.Select(s => s.ToStationDTO());
            }

            var stations = await _dbContext.Stations
                .Include(s => s.Coordinate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            await _cacheManager.SetDataAsync(cacheKey, stations, absoluteExpirationRelativeToNow: AbsoluteExpirationRelativeToNow, cancellationToken: cancellationToken);

            return stations.Select(s => s.ToStationDTO());
        }

        public async Task<StationDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"GetStationById_{id}";

            var cachedStation = await _cacheManager.GetCachedDataByKeyAsync<Station>(cacheKey, cancellationToken);

            if (cachedStation != null)
            {
                _logger.LogInformation("Retrieved station from cache.");
                return cachedStation.ToStationDTO();
            }

            var station = await _dbContext.Stations
                .Include(s => s.Coordinate)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (station == null) throw new Exception($"Station with id {id} not found.");

            await _cacheManager.SetDataAsync(cacheKey, station, absoluteExpirationRelativeToNow: AbsoluteExpirationRelativeToNow, cancellationToken: cancellationToken);

            return station.ToStationDTO();
        }

        public async Task<StationDTO> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"GetStationById_{id}";

            var cachedStation = await _cacheManager.GetCachedDataByKeyAsync<Station>(cacheKey, cancellationToken);

            if (cachedStation != null)
            {
                _logger.LogInformation("Retrieved station from cache.");
                return cachedStation.ToStationDTO();
            }

            var station = await _dbContext.Stations
                .Include(s => s.Coordinate)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ElementId == id, cancellationToken);

            if (station == null) throw new Exception($"Station with id {id} not found.");

            await _cacheManager.SetDataAsync(cacheKey, station, absoluteExpirationRelativeToNow: AbsoluteExpirationRelativeToNow, cancellationToken: cancellationToken);

            return station.ToStationDTO();
        }

        public async Task<OverpassResponseDTO> GetStationsDataAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting station data...");

            var result = await _overpassApiService.GetStationsDataAsync(cancellationToken);
            _logger.LogInformation("Successfully retrieved station data.");

            return result;
        }
    }
}
