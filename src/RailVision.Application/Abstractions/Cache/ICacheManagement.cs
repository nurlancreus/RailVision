namespace RailVision.Application.Abstractions.Cache
{
    public interface ICacheManagement
    {
        IEnumerable<string> GetAllCacheKeys();
        Task<IEnumerable<KeyValuePair<string, string>>> GetAllCacheKeysAndValuesAsync(CancellationToken cancellationToken = default);
        Task<Dictionary<string, string>> GetCacheEntryByKeyAsync(string key, CancellationToken cancellationToken = default);
        Task<bool> ClearAllCacheEntriesAsync(CancellationToken cancellationToken = default);
        Task<bool> ClearCacheEntryByKeyAsync(string key, CancellationToken cancellationToken = default);
        Task<T?> GetCachedDataByKeyAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
        Task SetDataAsync<T>(string key, T data, CancellationToken cancellationToken = default) where T : class;
    }
}
