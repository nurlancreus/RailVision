using RailVision.Application.Abstractions;
using RailVision.Application.Abstractions.Cache;

namespace RailVision.WebAPI.Endpoints
{
    public static class RedisCacheManagement
    {
        public static IEndpointRouteBuilder RegisterRedisCacheManagementEndpoints(this IEndpointRouteBuilder routes)
        {
            var cache = routes.MapGroup("api/cache/redis");

            cache.MapGet("", async (IRedisCacheManagement redisCacheManagement, CancellationToken cancellationToken) =>
            {
                var response = await redisCacheManagement.GetAllCacheKeysAndValuesAsync(cancellationToken);

                return Results.Ok(response);
            });

            cache.MapGet("keys", (IRedisCacheManagement redisCacheManagement) =>
            {
                var response = redisCacheManagement.GetAllCacheKeys();

                return Results.Ok(response);
            });

            cache.MapGet("{key}", async (string key, IRedisCacheManagement redisCacheManagement, CancellationToken cancellationToken) =>
            {
                var response = await redisCacheManagement.GetCacheEntryByKeyAsync(key, cancellationToken);
                return Results.Ok(response);
            });

            cache.MapDelete("", async (IRedisCacheManagement redisCacheManagement, CancellationToken cancellationToken) =>
            {
                await redisCacheManagement.ClearAllCacheEntriesAsync(cancellationToken);
                return Results.NoContent();
            });

            cache.MapDelete("{key}", async (string key, IRedisCacheManagement redisCacheManagement, CancellationToken cancellationToken) =>
            {
                await redisCacheManagement.ClearCacheEntryByKeyAsync(key, cancellationToken);
                return Results.NoContent();
            });

            return routes;
        }
    }
}
