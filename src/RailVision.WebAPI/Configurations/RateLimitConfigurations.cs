using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace RailVision.WebAPI.Configurations
{
    public static class RateLimitConfigurations
    {
        public static WebApplicationBuilder ConfigureRateLimiter(this WebApplicationBuilder builder)
        {
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddFixedWindowLimiter(policyName: "fixed", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 10;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 5;
                });
            });

            return builder;
        }
    }
}
