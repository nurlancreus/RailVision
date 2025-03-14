using Microsoft.AspNetCore.Mvc;
using RailVision.Application.Abstractions;
using System.Text.Json;

namespace RailVision.WebAPI.Endpoints
{
    public static class Terrains
    {
        public static IEndpointRouteBuilder RegisterTerrainsEndpoints(this IEndpointRouteBuilder routes)
        {
            var terrains = routes.MapGroup("api/terrains");

            terrains.MapGet("data", async ([FromQuery] string? type, ITerrainService terrainService, CancellationToken cancellationToken) =>
            {
                var response = await terrainService.GetTerrainObstaclesDataAsync(type, cancellationToken);

                return Results.Ok(response);
            });
            
            terrains.MapGet("coords", async (ITerrainService terrainService, CancellationToken cancellationToken) =>
            {
                var response = await terrainService.GetTerrainObstaclesCoordsAsync(cancellationToken);

                return Results.Ok(response);
            });

            return routes;
        }
    }
}
