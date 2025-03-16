using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RailVision.Application.Abstractions.Cache;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RailVision.Infrastructure.Services.Cache.InMemory
{
    public class InMemoryCacheManager(IMemoryCache cache) : IInMemoryCacheManager
    {
        private readonly IMemoryCache _cache = cache;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            ReferenceHandler = ReferenceHandler.Preserve
        };

        public Task<bool> ClearAllCacheEntriesAsync(CancellationToken cancellationToken = default)
        {
            var keys = GetAllCacheKeys();

            foreach (var key in keys)
            {
                _cache.Remove(key);
            }

            return Task.FromResult(true);
        }

        public Task<bool> ClearCacheEntryByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            _cache.Remove(key);

            return Task.FromResult(true);
        }

        public IEnumerable<string> GetAllCacheKeys()
        {
            if (_cache is MemoryCache memoryCache)
                return memoryCache.Keys.Select(k => k.ToString() ?? string.Empty);
            
            return [];
        }

        public Task<IEnumerable<KeyValuePair<string, string>>> GetAllCacheKeysAndValuesAsync(CancellationToken cancellationToken = default)
        {
            if (_cache is MemoryCache memoryCache)
            {
                var cacheEntries = memoryCache.Keys
                    .Select(k => new KeyValuePair<string, string>(
                        k.ToString() ?? string.Empty,
                        JsonSerializer.Serialize(memoryCache.Get(k), _jsonSerializerOptions)
                    ));

                return Task.FromResult(cacheEntries);
            }

            return Task.FromResult(Enumerable.Empty<KeyValuePair<string, string>>());
        }

        public Task<Dictionary<string, string>> GetCacheEntryByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(key, out var value))
            {
                return Task.FromResult(new Dictionary<string, string> { { key, JsonSerializer.Serialize(value, _jsonSerializerOptions) } });
            }

            throw new Exception("Cache entry not found.");
        }

        public Task<T?> GetCachedDataByKeyAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            if (_cache.TryGetValue(key, out var value))
            {
                return Task.FromResult(JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, _jsonSerializerOptions), _jsonSerializerOptions));
            }

            return Task.FromResult<T?>(null);
        }

        public Task SetDataAsync<T>(string key, T data, DateTimeOffset? absoluteExpiration = null, TimeSpan? absoluteExpirationRelativeToNow = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default) where T : class
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = absoluteExpiration,
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow,
                SlidingExpiration = slidingExpiration
            };

            _cache.Set(key, data, cacheEntryOptions);

            return Task.CompletedTask;
        }
    }
}