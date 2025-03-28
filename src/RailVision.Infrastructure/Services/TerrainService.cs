using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using RailVision.Application.Abstractions;
using RailVision.Application.Abstractions.Cache;
using RailVision.Application.Abstractions.OverpassAPI;
using RailVision.Application.DTOs;
using RailVision.Application.DTOs.Overpass;
using RailVision.Application.Helpers;
using RailVision.Domain.Entities;
using RailVision.Infrastructure.Persistence;

namespace RailVision.Infrastructure.Services
{
    public class TerrainService(ITerrainsOverpassApiService overpassApiService, AppDbContext dbContext, ICacheManagement cacheManager, ILogger<TerrainService> logger) : ITerrainService
    {
        private readonly ITerrainsOverpassApiService _overpassApiService = overpassApiService;
        private readonly AppDbContext _dbContext = dbContext;
        private readonly ILogger<TerrainService> _logger = logger;
        private readonly ICacheManagement _cacheManager = cacheManager;
        private readonly TimeSpan AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);

        public async Task<IEnumerable<ObstacleDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var cacheKey = "GetAllObstacles";

            var cachedObstacles = await _cacheManager.GetCachedDataByKeyAsync<List<Obstacle>>(cacheKey, cancellationToken);

            if (cachedObstacles != null)
            {
                _logger.LogInformation("Retrieved obstacles from cache.");
                return cachedObstacles.Select(o => o.ToObstacleDTO());
            }

            var obstacles = await _dbContext.Obstacles
                .Include(o => o.Coordinates)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            await _cacheManager.SetDataAsync(cacheKey, obstacles, absoluteExpirationRelativeToNow: AbsoluteExpirationRelativeToNow, cancellationToken: cancellationToken);

            return obstacles.Select(o => o.ToObstacleDTO());    
        }

        public async Task<ObstacleDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"GetObstacleById_{id}";

            var cachedObstacle = await _cacheManager.GetCachedDataByKeyAsync<Obstacle>(cacheKey, cancellationToken);

            if (cachedObstacle != null)
            {
                _logger.LogInformation("Retrieved obstacle from cache.");
                return cachedObstacle.ToObstacleDTO();
            }

            var obstacle = await _dbContext.Obstacles
                .Include(o => o.Coordinates)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

            if (obstacle == null) throw new Exception($"Obstacle with id {id} not found.");

            await _cacheManager.SetDataAsync(cacheKey, obstacle, absoluteExpirationRelativeToNow: AbsoluteExpirationRelativeToNow, cancellationToken: cancellationToken);

            return obstacle.ToObstacleDTO();
        }

        public async Task<ObstacleDTO> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"GetObstacleById_{id}";

            var cachedObstacle = await _cacheManager.GetCachedDataByKeyAsync<Obstacle>(cacheKey, cancellationToken);

            var obstacle = await _dbContext.Obstacles
                .Include(o => o.Coordinates)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.ElementId == id, cancellationToken);

            if (obstacle == null) throw new Exception($"Obstacle with id {id} not found.");

            await _cacheManager.SetDataAsync(cacheKey, obstacle, absoluteExpirationRelativeToNow: AbsoluteExpirationRelativeToNow, cancellationToken: cancellationToken);

            return obstacle.ToObstacleDTO();
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
