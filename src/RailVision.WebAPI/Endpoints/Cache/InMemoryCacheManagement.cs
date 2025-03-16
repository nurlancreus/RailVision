using RailVision.Application.Abstractions.Cache;

namespace RailVision.WebAPI.Endpoints.Cache
{
    public static class InMemoryCacheManagement
    {
        public static IEndpointRouteBuilder RegisterInMemoryCacheManagementEndpoints(this IEndpointRouteBuilder routes)
        {
            var cache = routes.MapGroup("api/cache/inmemory");

            cache.MapGet("", async (IInMemoryCacheManager inMemoryCacheManagement, CancellationToken cancellationToken) =>
            {
                var response = await inMemoryCacheManagement.GetAllCacheKeysAndValuesAsync(cancellationToken);

                return Results.Ok(response);
            });

            cache.MapGet("keys", (IInMemoryCacheManager inMemoryCacheManagement) =>
            {
                var response = inMemoryCacheManagement.GetAllCacheKeys();

                return Results.Ok(response);
            });

            cache.MapGet("{key}", async (string key, IInMemoryCacheManager inMemoryCacheManagement, CancellationToken cancellationToken) =>
            {
                var response = await inMemoryCacheManagement.GetCacheEntryByKeyAsync(key, cancellationToken);
                return Results.Ok(response);
            });

            cache.MapDelete("", async (IInMemoryCacheManager inMemoryCacheManagement, CancellationToken cancellationToken) =>
            {
                await inMemoryCacheManagement.ClearAllCacheEntriesAsync(cancellationToken);
                return Results.NoContent();
            });

            cache.MapDelete("{key}", async (string key, IInMemoryCacheManager inMemoryCacheManagement, CancellationToken cancellationToken) =>
            {
                await inMemoryCacheManagement.ClearCacheEntryByKeyAsync(key, cancellationToken);
                return Results.NoContent();
            });

            return routes;
        }
    }
}
