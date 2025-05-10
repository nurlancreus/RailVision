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
using System.Threading;
using RailVision.Domain.Entities.Coordinates;

namespace RailVision.Infrastructure.Services
{
    public class PopulationCenterService(IPopulationCentersOverpassApiService overpassApiService, AppDbContext dbContext, ICacheManagement cacheManager, ILogger<PopulationCenterService> logger) : IPopulationCenterService
    {
        private readonly IPopulationCentersOverpassApiService _overpassApiService = overpassApiService;
        private readonly AppDbContext _dbContext = dbContext;
        private readonly ICacheManagement _cacheManager = cacheManager;
        private readonly ILogger<PopulationCenterService> _logger = logger;
        private readonly TimeSpan AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);

        public async Task<IEnumerable<PopulationCenterDTO>> GetAllAsync(string? searchQuery, int? minPopulation, int? maxPopulation, bool? isDistinct = true, CancellationToken cancellationToken = default)
        {
            var cacheKey = "GetAllPopulationCenters";

            var cachedPopulationCenters = await _cacheManager.GetCachedDataByKeyAsync<List<PopulationCenterDTO>>(cacheKey, cancellationToken);

            if (cachedPopulationCenters != null)
            {
                cachedPopulationCenters = FilterByPopulation(cachedPopulationCenters, minPopulation, maxPopulation);

                cachedPopulationCenters = FilterByQuery(cachedPopulationCenters, searchQuery);

                _logger.LogInformation("Retrieved population centers from cache.");

                return isDistinct is true ? cachedPopulationCenters.DistinctBy(c => c.Name) : cachedPopulationCenters;
            }

            //var populationCenters = await _dbContext.PopulationCenters
            //    .Include(s => s.Coordinate)
            //    .AsNoTracking().ToListAsync(cancellationToken);

            var populationCenters = await GetPopulationCentersAsync(cancellationToken);

            await _cacheManager.SetDataAsync(cacheKey, populationCenters, absoluteExpirationRelativeToNow: AbsoluteExpirationRelativeToNow, cancellationToken: cancellationToken);

            populationCenters = FilterByPopulation(populationCenters, minPopulation, maxPopulation);

            populationCenters = FilterByQuery(populationCenters, searchQuery);

            return isDistinct is true ? populationCenters.DistinctBy(c => c.Name) : populationCenters;
        }

        private static List<PopulationCenterDTO> FilterByPopulation(List<PopulationCenterDTO> populationCenters, int? minPopulation, int? maxPopulation)
        {
            if (minPopulation.HasValue && maxPopulation.HasValue)
                return populationCenters.Where(c => c.Population >= minPopulation.Value && c.Population <= maxPopulation.Value).ToList();
            if (minPopulation.HasValue) return populationCenters.Where(c => c.Population >= minPopulation.Value).ToList();
            if (maxPopulation.HasValue) return populationCenters.Where(c => c.Population <= maxPopulation.Value).ToList();

            return populationCenters;
        }

        private static List<PopulationCenterDTO> FilterByQuery(List<PopulationCenterDTO> populationCenters, string? searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery)) return populationCenters;

            return populationCenters.Where(c => c.Name.Contains(searchQuery, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }

        public async Task<PopulationCenterDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"GetPopulationCenterById_{id}";

            var cachedPopulationCenter = await _cacheManager.GetCachedDataByKeyAsync<PopulationCenterDTO>(cacheKey, cancellationToken);

            if (cachedPopulationCenter != null)
            {
                _logger.LogInformation("Retrieved population center from cache.");
                return cachedPopulationCenter;
            }

            var populationCenters = await GetPopulationCentersAsync(cancellationToken);

            var populationCenter = populationCenters.FirstOrDefault(s => s.Id == id);

            if (populationCenter == null) throw new Exception($"Population Center with id {id} not found.");

            await _cacheManager.SetDataAsync(cacheKey, populationCenter, absoluteExpirationRelativeToNow: AbsoluteExpirationRelativeToNow, cancellationToken: cancellationToken);

            return populationCenter;
        }

        public async Task<PopulationCenterDTO> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"GetPopulationCenterById_{id}";

            var cachedPopulationCenter = await _cacheManager.GetCachedDataByKeyAsync<PopulationCenterDTO>(cacheKey, cancellationToken);

            if (cachedPopulationCenter != null)
            {
                _logger.LogInformation("Retrieved population center from cache.");
                return cachedPopulationCenter;
            }

            var populationCenters = await GetPopulationCentersAsync(cancellationToken);

            var populationCenter = populationCenters.FirstOrDefault(s => s.ElementId == id);

            if (populationCenter == null) throw new Exception($"Population center with id {id} not found.");

            await _cacheManager.SetDataAsync(cacheKey, populationCenter, absoluteExpirationRelativeToNow: AbsoluteExpirationRelativeToNow, cancellationToken: cancellationToken);

            return populationCenter;
        }

        public async Task<OverpassResponseDTO> GetPopulationCentersDataAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting population center data...");

            var result = await _overpassApiService.GetPopulationCentersAsync(cancellationToken);
            _logger.LogInformation("Successfully retrieved population center data.");

            return result;
        }

        private async Task<List<PopulationCenterDTO>> GetPopulationCentersAsync(CancellationToken cancellationToken)
        {
            var populationCentersData = await _overpassApiService.GetPopulationCentersAsync(cancellationToken);

            if (populationCentersData == null || populationCentersData.Elements == null || populationCentersData.Elements.Count == 0)
            {
                throw new Exception("No population centers data found.");
            }

            var populationCenters = populationCentersData.Elements
                .Where(e => e.Tags.ContainsKey("population"));

            var populationCentersDtos = populationCenters.Select(e =>
            {
                var pc = new PopulationCenterDTO
                {
                    Id = Guid.NewGuid(),
                    ElementId = e.Id,
                    Name = e.Tags.TryGetValue("name", out var name) ? name : "Unknown",
                    Population = e.Tags.TryGetValue("population", out var population) ? int.Parse(population) : 0
                };

                if (e.Type == "node")
                {
                    pc.Coordinate = new CoordinateDTO
                    {
                        Latitude = e.Lat ?? 0,
                        Longitude = e.Lon ?? 0
                    };
                }
                else
                {
                    pc.Coordinate = new CoordinateDTO
                    {
                        Latitude = (e.Bounds.GetValueOrDefault("minlat", 0) + e.Bounds.GetValueOrDefault("maxlat", 0)) / 2,
                        Longitude = (e.Bounds.GetValueOrDefault("minlon", 0) + e.Bounds.GetValueOrDefault("maxlon", 0)) / 2
                    };
                }

                return pc;
            })
                .ToList();

            return populationCentersDtos;
        }
    }
}
