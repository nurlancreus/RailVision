using Microsoft.AspNetCore.Mvc;
using RailVision.Application.Abstractions;
using RailVision.Application.DTOs.Route;

namespace RailVision.WebAPI.Endpoints
{
    public static class Routes
    {
        public static IEndpointRouteBuilder RegisterRoutesEndpoints(this IEndpointRouteBuilder routes)
        {
            var appRoutes = routes.MapGroup("api/routes");

            appRoutes.MapPost("", async (IRouteService routeService, [FromBody] RouteRequestDTO request, CancellationToken cancellationToken) =>
            {
                var response = await routeService.DrawOptimalRouteAsync(request, cancellationToken);

                return Results.Ok(response);
            });

            return routes;
        }
    }
}
