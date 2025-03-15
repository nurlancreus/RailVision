using RailVision.WebAPI.Middlewares;
using StackExchange.Redis;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace RailVision.WebAPI.Configurations
{
    public static class CachingConfigurations
    {
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
