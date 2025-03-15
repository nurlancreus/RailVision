
using Microsoft.AspNetCore.Mvc;

namespace RailVision.WebAPI.Middlewares
{
    public class CustomResponseCachingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var endpointMetadata = context.GetEndpoint()?.Metadata;

            if (endpointMetadata is not null)
            {
                var responseCache = endpointMetadata.FirstOrDefault(x => x is ResponseCacheAttribute);

                if (responseCache is ResponseCacheAttribute attribute)
                {
                    var cacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        MaxAge = TimeSpan.FromSeconds(attribute.Duration)
                    };

                    if (attribute.Location == ResponseCacheLocation.Any)
                    {
                        cacheControl.Public = true;
                    }
                    else if (attribute.Location == ResponseCacheLocation.Client)
                    {
                        cacheControl.Private = true;
                    }
                    else
                    {
                        cacheControl.NoStore = true;
                    }

                    context.Response.GetTypedHeaders().CacheControl = cacheControl;

                    context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
                        new string[] { attribute.VaryByHeader ?? string.Empty };
                }
            }

            await next(context);
        }

    }
    public static class CustomResponseCachingMiddlewareExtension
    {
        public static IApplicationBuilder UseCustomResponseCachingMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CustomResponseCachingMiddleware>();
        }
    }
}
