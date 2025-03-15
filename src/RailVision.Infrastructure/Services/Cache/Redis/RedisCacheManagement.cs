using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RailVision.Application.Abstractions.Cache;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RailVision.Infrastructure.Services.Cache.Redis
{
    public class RedisCacheManagement(IDistributedCache distributedCache, IConnectionMultiplexer redisConnection, ILogger<RedisCacheManagement> logger, IConfiguration configuration) : IRedisCacheManagement
    {
        private readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            ReferenceHandler = ReferenceHandler.Preserve
        };
        private readonly IDistributedCache _cache = distributedCache;
        private readonly IConnectionMultiplexer _redisConnection = redisConnection;
        private readonly ILogger<RedisCacheManagement> _logger = logger;
        private readonly IConfiguration _configuration = configuration;

        public async Task<bool> ClearAllCacheEntriesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Clearing all cache entries...");

            var server = _redisConnection.GetServer(_redisConnection.GetEndPoints().First());

            foreach (var key in server.Keys())
            {
                string instanceName = _configuration["RedisCacheOptions:InstanceName"] ?? string.Empty;
                var keyWithoutPrefix = key.ToString().Replace($"{instanceName}", "");

                _logger.LogDebug("Removing cache entry with key: {Key}", keyWithoutPrefix);
                await _cache.RemoveAsync(keyWithoutPrefix, cancellationToken);
            }

            _logger.LogInformation("All cache entries cleared successfully.");
            return true;
        }

        public async Task<bool> ClearCacheEntryByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Clearing cache entry with key: {Key}", key);

            await _cache.RemoveAsync(key, cancellationToken);

            _logger.LogInformation("Cache entry with key {Key} cleared successfully.", key);
            return true;
        }

        public IEnumerable<string> GetAllCacheKeys()
        {
            var server = _redisConnection.GetServer(_redisConnection.GetEndPoints().First());
            return server.Keys().Select(k => k.ToString());
        }

        public async Task<IEnumerable<KeyValuePair<string, string>>> GetAllCacheKeysAndValuesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving all cache keys and values...");

            var server = _redisConnection.GetServer(_redisConnection.GetEndPoints().First());
            var keys = server.Keys().ToArray();
            string instanceName = _configuration["RedisCacheOptions:InstanceName"] ?? string.Empty;

            var cacheEntries = new List<KeyValuePair<string, string>>();

            foreach (var key in keys)
            {
                var keyWithoutPrefix = key.ToString().Replace($"{instanceName}", "");
                var value = await _cache.GetStringAsync(keyWithoutPrefix, cancellationToken);

                _logger.LogDebug("Retrieved cache entry: Key = {Key}, Value = {Value}", keyWithoutPrefix, value ?? "null");
                cacheEntries.Add(new KeyValuePair<string, string>(keyWithoutPrefix, value ?? "null"));
            }

            _logger.LogInformation("Retrieved {Count} cache entries.", cacheEntries.Count);
            return cacheEntries;
        }

        public async Task<Dictionary<string, string>> GetCacheEntryByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving cache entry with key: {Key}", key);

            var value = await _cache.GetStringAsync(key, cancellationToken);

            if (value == null)
            {
                _logger.LogWarning("Cache entry with key {Key} not found.", key);
                throw new Exception("Cache entry not found.");
            }

            _logger.LogDebug("Retrieved cache entry: Key = {Key}, Value = {Value}", key, value);
            return new() { { key, value } };
        }

        public async Task<T?> GetCachedDataByKeyAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            _logger.LogInformation("Retrieving cached data for key: {Key}", key);

            var value = await _cache.GetStringAsync(key, cancellationToken);

            if (value == null)
            {
                _logger.LogWarning("Cached data for key {Key} not found.", key);
                return null;
            }

            _logger.LogDebug("Retrieved cached data for key {Key}: {Value}", key, value);
            return JsonSerializer.Deserialize<T>(value, jsonSerializerOptions);
        }

        public async Task SetDataAsync<T>(string key, T data, CancellationToken cancellationToken = default) where T : class
        {
            _logger.LogInformation("Setting data in cache for key: {Key}", key);

            var serializedData = JsonSerializer.Serialize(data, jsonSerializerOptions);
            await _cache.SetStringAsync(key, serializedData, cancellationToken);

            _logger.LogDebug("Data set in cache for key {Key}: {Value}", key, serializedData);
        }

        public async Task SetDataAsync<T>(string key, T data, DistributedCacheEntryOptions distributedCacheEntryOptions, CancellationToken cancellationToken = default) where T : class
        {
            _logger.LogInformation("Setting data in cache for key: {Key} with custom options", key);

            var serializedData = JsonSerializer.Serialize(data, jsonSerializerOptions);
            await _cache.SetStringAsync(key, serializedData, distributedCacheEntryOptions, cancellationToken);

            _logger.LogDebug("Data set in cache for key {Key} with custom options: {Value}", key, serializedData);
        }
    }
}