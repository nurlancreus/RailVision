using RailVision.Application.Abstractions;
using RailVision.Domain.Entities;

namespace RailVision.WebAPI.Endpoints
{
    public static class Stations
    {
        public static IEndpointRouteBuilder RegisterStationsEndpoints(this IEndpointRouteBuilder routes)
        {
            var stations = routes.MapGroup("api/stations");

            stations.MapGet("data", async (IStationService stationService, CancellationToken cancellationToken) =>
            {
                var response = await stationService.GetStationsDataAsync(cancellationToken);

                return Results.Ok(response);
            });

            stations.MapGet("", async (IStationService stationService, CancellationToken cancellationToken) =>
            {
                var response = await stationService.GetAllAsync(cancellationToken);

                return Results.Ok(response);
            });

            stations.MapGet("{id:guid}", async (Guid id, IStationService stationService, CancellationToken cancellationToken) =>
            {
                var response = await stationService.GetByIdAsync(id, cancellationToken);

                return Results.Ok(response);
            });
            
            stations.MapGet("{id:long}", async (long id, IStationService stationService, CancellationToken cancellationToken) =>
            {
                var response = await stationService.GetByIdAsync(id, cancellationToken);

                return Results.Ok(response);
            });

            return routes;
        }
    }
}
