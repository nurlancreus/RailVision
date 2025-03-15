using Microsoft.AspNetCore.Mvc;
using RailVision.Application.Abstractions;
using RailVision.Domain.Entities;
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

            terrains.MapGet("", async (ITerrainService terrainService, CancellationToken cancellationToken) =>
            {
                var response = await terrainService.GetAllAsync(cancellationToken);

                return Results.Ok(response);
            });

            terrains.MapGet("{id}", async (Guid id, ITerrainService terrainService, CancellationToken cancellationToken) =>
            {
                var response = await terrainService.GetByIdAsync(id, cancellationToken);

                return Results.Ok(response);
            });

            terrains.MapGet("{id:long}", async (long id, ITerrainService terrainService, CancellationToken cancellationToken) =>
            {
                var response = await terrainService.GetByIdAsync(id, cancellationToken);

                return Results.Ok(response);
            });

            return routes;
        }
    }
}
