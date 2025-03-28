using Microsoft.Extensions.Logging;
using RailVision.Application.Abstractions.Cache;

namespace RailVision.Infrastructure.Services.Cache
{
    public class CacheManagement(ICacheManager cacheManager, ILogger<CacheManagement> logger) : ICacheManagement
    {
        private readonly ICacheManager _cacheManager = cacheManager;
        private readonly ILogger<CacheManagement> _logger = logger;

        public async Task<bool> ClearAllCacheEntriesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Clearing all cache entries.");

            var result = await _cacheManager.ClearAllCacheEntriesAsync(cancellationToken);

            _logger.LogInformation("All cache entries cleared successfully: {Result}", result);

            return result;
        }

        public async Task<bool> ClearCacheEntryByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Clearing cache entry for key: {Key}", key);

            var result = await _cacheManager.ClearCacheEntryByKeyAsync(key, cancellationToken);

            _logger.LogInformation("Cache entry cleared for key {Key}: {Result}", key, result);

            return result;
        }

        public IEnumerable<string> GetAllCacheKeys()
        {
            _logger.LogInformation("Retrieving all cache keys.");

            var keys = _cacheManager.GetAllCacheKeys();

            _logger.LogInformation("Retrieved {Count} cache keys.", keys.Count());

            return keys;
        }

        public async Task<IEnumerable<KeyValuePair<string, string>>> GetAllCacheKeysAndValuesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving all cache keys and values.");

            var cacheEntries = await _cacheManager.GetAllCacheKeysAndValuesAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} cache entries.", cacheEntries.Count());

            return cacheEntries;
        }

        public async Task<T?> GetCachedDataByKeyAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            _logger.LogInformation("Retrieving cached data for key: {Key}", key);

            var data = await _cacheManager.GetCachedDataByKeyAsync<T>(key, cancellationToken);

            _logger.LogInformation("Cached data retrieved for key {Key}: {Data}", key, data != null ? "Exists" : "Null");

            return data;
        }

        public async Task<Dictionary<string, string>> GetCacheEntryByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving cache entry for key: {Key}", key);

            var cacheEntry = await _cacheManager.GetCacheEntryByKeyAsync(key, cancellationToken);

            _logger.LogInformation("Cache entry retrieved for key {Key}: {Entry}", key, cacheEntry);

            return cacheEntry;
        }

        public async Task SetDataAsync<T>(string key, T data, DateTimeOffset? absoluteExpiration = null, TimeSpan? absoluteExpirationRelativeToNow = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default) where T : class
        {
            _logger.LogInformation("Setting data in cache for key: {Key}", key);

            await _cacheManager.SetDataAsync(key, data, absoluteExpiration, absoluteExpirationRelativeToNow, slidingExpiration, cancellationToken);

            _logger.LogInformation("Data set in cache for key {Key}", key);
        }
    }
}