using Microsoft.AspNetCore.Mvc;

namespace RailVision.WebAPI
{
    public static class RouteHandlerBuilders
    {
        public static RouteHandlerBuilder WithResponseCache(this RouteHandlerBuilder builder, int durationSeconds = 60, string? varyByHeader = null)
        {
            builder.Add(endpointBuilder =>
            {
                var responseCacheAttribute = new ResponseCacheAttribute()
                {
                    VaryByHeader = varyByHeader,
                    Duration = durationSeconds
                };

                endpointBuilder.Metadata.Add(responseCacheAttribute);
            });

            return builder;
        }
    }
}
