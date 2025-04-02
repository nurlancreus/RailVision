using Microsoft.AspNetCore.Mvc;
using RailVision.Application.Abstractions;
using RailVision.Application.DTOs;
using RailVision.Application.DTOs.Route;

namespace RailVision.WebAPI.Endpoints
{
    public static class Routes
    {
        public static IEndpointRouteBuilder RegisterRoutesEndpoints(this IEndpointRouteBuilder routes)
        {
            var appRoutes = routes.MapGroup("api/routes");

            appRoutes.MapGet("", async (IRouteService routeService, [FromQuery] long? FromId, [FromQuery] long? ToId, [FromQuery] double? FromLat, [FromQuery] double? FromLon, [FromQuery] double? ToLat, [FromQuery] double? ToLon, CancellationToken cancellationToken) =>
            {
                var request = new RouteRequestDTO
                {
                    FromId = FromId,
                    ToId = ToId,
                    FromCoordinate = (FromLat.HasValue && FromLon.HasValue) ? new CoordinateDTO { Latitude = FromLat.Value, Longitude = FromLon.Value } : null,
                    ToCoordinate = (ToLat.HasValue && ToLon.HasValue) ? new CoordinateDTO { Latitude = ToLat.Value, Longitude = ToLon.Value } : null
                };

                var response = await routeService.DrawOptimalRouteAsync(request, cancellationToken);

                return Results.Ok(response);
            });


            return routes;
        }
    }
}
