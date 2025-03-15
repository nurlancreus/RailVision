using RailVision.Application.Abstractions;
using RailVision.Application.DTOs.Overpass;
using RailVision.Application.DTOs;
using Microsoft.Extensions.Logging;
using RailVision.Application.Abstractions.OverpassAPI;
using RailVision.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RailVision.Application;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using RailVision.Domain.Entities;
using RailVision.Application.Abstractions.Cache;

namespace RailVision.Infrastructure.Services
{
    public class RailwayService(IRailwaysOverpassApiService overpassApiService, AppDbContext dbContext, IRedisCacheManagement cacheManagement, ILogger<RailwayService> logger) : IRailwayService
    {
        private readonly IRailwaysOverpassApiService _overpassApiService = overpassApiService;
        private readonly AppDbContext _dbContext = dbContext;
        private readonly IRedisCacheManagement _cacheManagement = cacheManagement;
        private readonly ILogger<RailwayService> _logger = logger;
        private readonly DistributedCacheEntryOptions distributedCacheEntryOptions = new ()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1),
        };

        public async Task<OverpassResponseDTO> GetRailwaysDataAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting railway data...");

            var result = await _overpassApiService.GetRailwaysDataAsync(cancellationToken);
            _logger.LogInformation("Successfully retrieved railway data.");
            return result;
        }

        public async Task<IEnumerable<RailwayLineDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var cacheKey = "GetAllRailways";

            var cachedRailways = await _cacheManagement.GetCachedDataByKeyAsync<List<Railway>>(cacheKey, cancellationToken);

            if (cachedRailways != null)
            {
                _logger.LogInformation("Retrieved railways from cache.");
                return cachedRailways.Select(r => r.ToRailwayDTO());
            }

            var railwayLines = await _dbContext.Railways
                                .Include(r => r.Coordinates)
                                .AsNoTracking()
                                .ToListAsync(cancellationToken);


            await _cacheManagement.SetDataAsync(cacheKey, railwayLines, distributedCacheEntryOptions, cancellationToken);

            return railwayLines.Select(r => r.ToRailwayDTO());
        }

        public async Task<RailwayLineDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"GetRailwayById_{id}";

            var cachedRailway = await _cacheManagement.GetCachedDataByKeyAsync<Railway>(cacheKey, cancellationToken);

            if (cachedRailway != null)
            {
                _logger.LogInformation("Retrieved railway from cache.");

                return cachedRailway.ToRailwayDTO();
            }

            var railway = await _dbContext.Railways
                .Include(r => r.Coordinates)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (railway == null)
                throw new Exception($"Railway line with id {id} not found.");


            await _cacheManagement.SetDataAsync(cacheKey, railway, distributedCacheEntryOptions, cancellationToken);

            return railway.ToRailwayDTO();
        }

        public async Task<RailwayLineDTO> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"GetRailwayById_{id}";

            var cachedRailway = await _cacheManagement.GetCachedDataByKeyAsync<Railway>(cacheKey, cancellationToken);

            if (cachedRailway != null)
            {
                _logger.LogInformation("Retrieved railway from cache.");

                return cachedRailway.ToRailwayDTO();
            }

            var railway = await _dbContext.Railways
                .Include(r => r.Coordinates)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ElementId == id, cancellationToken);

            if (railway == null)
                throw new Exception($"Railway line with id {id} not found.");

            await _cacheManagement.SetDataAsync(cacheKey, railway, distributedCacheEntryOptions, cancellationToken);

            return railway.ToRailwayDTO();
        }
    }
}
