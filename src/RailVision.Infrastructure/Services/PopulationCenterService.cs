using Microsoft.Extensions.Logging;
using RailVision.Application.Abstractions;
using RailVision.Application.Abstractions.Cache;
using RailVision.Application.Abstractions.OverpassAPI;
using RailVision.Application.DTOs.Overpass;
using RailVision.Application.DTOs;
using RailVision.Domain.Entities;
using RailVision.Infrastructure.Persistence;
using RailVision.Application.Helpers;
using Microsoft.EntityFrameworkCore;

namespace RailVision.Infrastructure.Services
{
    public class PopulationCenterService(IPopulationCentersOverpassApiService overpassApiService, AppDbContext dbContext, ICacheManagement cacheManager, ILogger<PopulationCenterService> logger) : IPopulationCenterService
    {
        private readonly IPopulationCentersOverpassApiService _overpassApiService = overpassApiService;
        private readonly AppDbContext _dbContext = dbContext;
        private readonly ICacheManagement _cacheManager = cacheManager;
        private readonly ILogger<PopulationCenterService> _logger = logger;
        private readonly TimeSpan AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);

        public async Task<IEnumerable<PopulationCenterDTO>> GetAllAsync(int? minPopulation, int? maxPopulation, CancellationToken cancellationToken = default)
        {
            var cacheKey = "GetAllPopulationCenters";

            var cachedPopulationCenters = await _cacheManager.GetCachedDataByKeyAsync<IEnumerable<PopulationCenter>>(cacheKey, cancellationToken);

            if (cachedPopulationCenters != null)
            {
                cachedPopulationCenters = FilterByPopulation(cachedPopulationCenters, minPopulation, maxPopulation).ToList();

                _logger.LogInformation("Retrieved population centers from cache.");
                return cachedPopulationCenters.Select(s => s.ToPopulationCenterDTO());
            }

            var populationCenters = await _dbContext.PopulationCenters
                .Include(s => s.Coordinate)
                .AsNoTracking().ToListAsync(cancellationToken);

            await _cacheManager.SetDataAsync(cacheKey, populationCenters, absoluteExpirationRelativeToNow: AbsoluteExpirationRelativeToNow, cancellationToken: cancellationToken);

            populationCenters = FilterByPopulation(populationCenters, minPopulation, maxPopulation).ToList();

            return populationCenters.Select(s => s.ToPopulationCenterDTO());
        }

        private static IEnumerable<PopulationCenter> FilterByPopulation(IEnumerable<PopulationCenter> populationCenters, int? minPopulation, int? maxPopulation)
        {
            if (minPopulation.HasValue && maxPopulation.HasValue)
                return populationCenters.Where(s => s.Population >= minPopulation.Value && s.Population <= maxPopulation.Value);
            if (minPopulation.HasValue) return populationCenters.Where(s => s.Population >= minPopulation.Value);
            if (maxPopulation.HasValue) return populationCenters.Where(s => s.Population <= maxPopulation.Value);

            return populationCenters;
        }

        public async Task<PopulationCenterDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"GetPopulationCenterById_{id}";

            var cachedPopulationCenter = await _cacheManager.GetCachedDataByKeyAsync<PopulationCenter>(cacheKey, cancellationToken);

            if (cachedPopulationCenter != null)
            {
                _logger.LogInformation("Retrieved population center from cache.");
                return cachedPopulationCenter.ToPopulationCenterDTO();
            }

            var populationCenter = await _dbContext.PopulationCenters
                .Include(s => s.Coordinate)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (populationCenter == null) throw new Exception($"Population Center with id {id} not found.");

            await _cacheManager.SetDataAsync(cacheKey, populationCenter, absoluteExpirationRelativeToNow: AbsoluteExpirationRelativeToNow, cancellationToken: cancellationToken);

            return populationCenter.ToPopulationCenterDTO();
        }

        public async Task<PopulationCenterDTO> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"GetPopulationCenterById_{id}";

            var cachedPopulationCenter = await _cacheManager.GetCachedDataByKeyAsync<PopulationCenter>(cacheKey, cancellationToken);

            if (cachedPopulationCenter != null)
            {
                _logger.LogInformation("Retrieved population center from cache.");
                return cachedPopulationCenter.ToPopulationCenterDTO();
            }

            var populationCenter = await _dbContext.PopulationCenters
                .Include(s => s.Coordinate)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ElementId == id, cancellationToken);

            if (populationCenter == null) throw new Exception($"Population center with id {id} not found.");

            await _cacheManager.SetDataAsync(cacheKey, populationCenter, absoluteExpirationRelativeToNow: AbsoluteExpirationRelativeToNow, cancellationToken: cancellationToken);

            return populationCenter.ToPopulationCenterDTO();
        }

        public async Task<OverpassResponseDTO> GetPopulationCentersDataAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting population center data...");

            var result = await _overpassApiService.GetPopulationCentersAsync(cancellationToken);
            _logger.LogInformation("Successfully retrieved population center data.");

            return result;
        }
    }
}
