using RailVision.WebAPI.Middlewares;
using StackExchange.Redis;
using System.Text.Json.Serialization;
using System.Text.Json;
using RailVision.Application.Abstractions.Cache;
using RailVision.Infrastructure.Services.Cache;
using RailVision.Infrastructure.Services.Cache.Redis;
using RailVision.Infrastructure.Services.Cache.InMemory;

namespace RailVision.WebAPI.Configurations
{
    public static class CachingConfigurations
    {
        public static WebApplicationBuilder EnableCaching<T>(this WebApplicationBuilder builder) where T : class, ICacheManager
        {
            // Register Cache Manager
            builder.Services.AddSingleton<ICacheManager, T>();

            // Register Cache Manager Service
            builder.Services.AddScoped<ICacheManagerService, CacheManagerService>();

            // Register Redis Cache Manager
            builder.Services.AddScoped<IRedisCacheManager, RedisCacheManager>();

            // Register In-Memory Cache Manager
            builder.Services.AddScoped<IInMemoryCacheManager, InMemoryCacheManager>();

            return builder;
        }
        public static WebApplicationBuilder ConfigureInMemoryCaching(this WebApplicationBuilder builder)
        {
            // Enable In-Memory Caching
            builder.Services.AddMemoryCache();

            return builder;
        }
        public static WebApplicationBuilder ConfigureRedis(this WebApplicationBuilder builder)
        {
            // Register Redis distributed cache
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                // Redis Configuration
                options.Configuration = builder.Configuration["RedisCacheOptions:Configuration"];

                // Redis Instance Name
                options.InstanceName = builder.Configuration["RedisCacheOptions:InstanceName"];
            });

            // Register Redis Connection Multiplexer
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>

                // Connect to Redis
                ConnectionMultiplexer.Connect(builder.Configuration["RedisCacheOptions:Configuration"] ?? string.Empty));
            return builder;
        }

        public static WebApplicationBuilder ConfigureResponseCaching(this WebApplicationBuilder builder)
        {
            // Registering Custom Response Caching Middleware
            builder.Services.AddTransient<CustomResponseCachingMiddleware>();
            // Registering Response Caching Service
            builder.Services.AddResponseCaching(options =>
            {
                options.UseCaseSensitivePaths = true;
            });
            return builder;
        }
    }
}
