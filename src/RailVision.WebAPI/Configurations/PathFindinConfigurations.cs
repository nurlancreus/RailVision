using RailVision.Application.Abstractions.PathFinding;
using RailVision.Infrastructure.Services.PathFinding;

namespace RailVision.WebAPI.Configurations
{
    public static class PathFindinConfigurations
    {
        public static WebApplicationBuilder SetPathFindingStrategy<T>(this WebApplicationBuilder builder) where T : class, IPathFindingStrategy
        {
            builder.Services.AddScoped<IPathFindingStrategy, T>();

            return builder;
        }
    }
}
