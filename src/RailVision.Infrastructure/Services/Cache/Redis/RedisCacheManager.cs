using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RailVision.Application.Abstractions.Cache;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RailVision.Infrastructure.Services.Cache.Redis
{
    public class RedisCacheManager(IDistributedCache distributedCache, IConnectionMultiplexer redisConnection, IConfiguration configuration) : IRedisCacheManager
    {
        private readonly IDistributedCache _cache = distributedCache;
        private readonly IConnectionMultiplexer _redisConnection = redisConnection;
        private readonly IConfiguration _configuration = configuration;
        private readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            ReferenceHandler = ReferenceHandler.Preserve
        };

        public async Task<bool> ClearAllCacheEntriesAsync(CancellationToken cancellationToken = default)
        {
            var server = _redisConnection.GetServer(_redisConnection.GetEndPoints().First());

            foreach (var key in server.Keys())
            {
                string instanceName = _configuration["RedisCacheOptions:InstanceName"] ?? string.Empty;
                var keyWithoutPrefix = key.ToString().Replace($"{instanceName}", "");

                await _cache.RemoveAsync(keyWithoutPrefix, cancellationToken);
            }

            return true;
        }

        public async Task<bool> ClearCacheEntryByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(key, cancellationToken);

            return true;
        }

        public IEnumerable<string> GetAllCacheKeys()
        {
            var server = _redisConnection.GetServer(_redisConnection.GetEndPoints().First());
            return server.Keys().Select(k => k.ToString());
        }

        public async Task<IEnumerable<KeyValuePair<string, string>>> GetAllCacheKeysAndValuesAsync(CancellationToken cancellationToken = default)
        {
            var server = _redisConnection.GetServer(_redisConnection.GetEndPoints().First());
            var keys = server.Keys().ToArray();
            string instanceName = _configuration["RedisCacheOptions:InstanceName"] ?? string.Empty;

            var cacheEntries = new List<KeyValuePair<string, string>>();

            foreach (var key in keys)
            {
                var keyWithoutPrefix = key.ToString().Replace($"{instanceName}", "");
                var value = await _cache.GetStringAsync(keyWithoutPrefix, cancellationToken);

                cacheEntries.Add(new KeyValuePair<string, string>(keyWithoutPrefix, value ?? "null"));
            }
            return cacheEntries;
        }

        public async Task<Dictionary<string, string>> GetCacheEntryByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            var value = await _cache.GetStringAsync(key, cancellationToken);

            if (value == null)
                throw new Exception("Cache entry not found.");
            
            return new() { { key, value } };
        }

        public async Task<T?> GetCachedDataByKeyAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            var value = await _cache.GetStringAsync(key, cancellationToken);

            if (value == null) return null;
            
            return JsonSerializer.Deserialize<T>(value, jsonSerializerOptions);
        }

        public async Task SetDataAsync<T>(string key, T data, DateTimeOffset? absoluteExpiration = null, TimeSpan? absoluteExpirationRelativeToNow = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default) where T : class
        {

            var distributedCacheEntryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = absoluteExpiration,
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow,
                SlidingExpiration = slidingExpiration
            };

            var serializedData = JsonSerializer.Serialize(data, jsonSerializerOptions);
            await _cache.SetStringAsync(key, serializedData, distributedCacheEntryOptions, cancellationToken);        }
    }
}