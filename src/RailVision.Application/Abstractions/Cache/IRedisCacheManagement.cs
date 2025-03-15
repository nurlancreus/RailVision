using Microsoft.Extensions.Caching.Distributed;

namespace RailVision.Application.Abstractions.Cache
{
    public interface IRedisCacheManagement : ICacheManagement
    {
        Task SetDataAsync<T>(string key, T data, DistributedCacheEntryOptions distributedCacheEntryOptions, CancellationToken cancellationToken = default) where T : class;
    }
}
