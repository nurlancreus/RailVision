using RailVision.Application.Abstractions;
using RailVision.Application.DTOs.Overpass;
using RailVision.Application.DTOs;
using Microsoft.Extensions.Logging;
using RailVision.Application.Abstractions.OverpassAPI;
using RailVision.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RailVision.Application;
using RailVision.Application.Abstractions.Cache;
using RailVision.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace RailVision.Infrastructure.Services
{
    public class StationService(IStationsOverpassApiService overpassApiService, AppDbContext dbContext, IRedisCacheManagement cacheManagement, ILogger<StationService> logger) : IStationService
    {
        private readonly IStationsOverpassApiService _overpassApiService = overpassApiService;
        private readonly AppDbContext _dbContext = dbContext;
        private readonly IRedisCacheManagement _cacheManagement = cacheManagement;
        private readonly ILogger<StationService> _logger = logger;
        private readonly DistributedCacheEntryOptions distributedCacheEntryOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1),
        };

        public async Task<IEnumerable<StationDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var cacheKey = "GetAllStations";

            var cachedStations = await _cacheManagement.GetCachedDataByKeyAsync<List<Station>>(cacheKey, cancellationToken);

            if (cachedStations != null)
            {
                _logger.LogInformation("Retrieved stations from cache.");
                return cachedStations.Select(s => s.ToStationDTO());
            }

            var stations = await _dbContext.Stations
                .Include(s => s.Coordinate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            await _cacheManagement.SetDataAsync(cacheKey, stations, distributedCacheEntryOptions, cancellationToken);

            return stations.Select(s => s.ToStationDTO());
        }

        public async Task<StationDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"GetStationById_{id}";

            var cachedStation = await _cacheManagement.GetCachedDataByKeyAsync<Station>(cacheKey, cancellationToken);

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

            await _cacheManagement.SetDataAsync(cacheKey, station, distributedCacheEntryOptions, cancellationToken);

            return station.ToStationDTO();
        }

        public async Task<StationDTO> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"GetStationById_{id}";

            var cachedStation = await _cacheManagement.GetCachedDataByKeyAsync<Station>(cacheKey, cancellationToken);

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

            await _cacheManagement.SetDataAsync(cacheKey, station, distributedCacheEntryOptions, cancellationToken);

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
