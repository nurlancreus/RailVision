using Microsoft.AspNetCore.Mvc;
using RailVision.Application.Abstractions;

namespace RailVision.WebAPI.Endpoints
{
    public static class PopulationCenters
    {
        public static IEndpointRouteBuilder RegisterPopulationCentersEndpoints(this IEndpointRouteBuilder routes)
        {
            var populationCenters = routes.MapGroup("api/populationCenters");

            populationCenters.MapGet("data", async (IPopulationCenterService populationCenterService, CancellationToken cancellationToken) =>
            {
                var response = await populationCenterService.GetPopulationCentersDataAsync(cancellationToken);

                return Results.Ok(response);
            });

            populationCenters.MapGet("", async (IPopulationCenterService populationCenterService, [FromQuery] string? searchQuery, [FromQuery] int? minPopulation, [FromQuery] int? maxPopulation, CancellationToken cancellationToken) =>
            {
                var response = await populationCenterService.GetAllAsync(searchQuery, minPopulation, maxPopulation, cancellationToken);

                return Results.Ok(response);
            });

            populationCenters.MapGet("{id:guid}", async (Guid id, IPopulationCenterService populationCenterService, CancellationToken cancellationToken) =>
            {
                var response = await populationCenterService.GetByIdAsync(id, cancellationToken);

                return Results.Ok(response);
            });

            populationCenters.MapGet("{id:long}", async (long id, IPopulationCenterService populationCenterService, CancellationToken cancellationToken) =>
            {
                var response = await populationCenterService.GetByIdAsync(id, cancellationToken);

                return Results.Ok(response);
            });

            return routes;
        }
    }
}
