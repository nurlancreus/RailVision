using RailVision.Application.Abstractions;

namespace RailVision.WebAPI.Endpoints
{
    public static class Stations
    {
        public static IEndpointRouteBuilder RegisterStationsEndpoints(this IEndpointRouteBuilder routes)
        {
            var stations = routes.MapGroup("api/stations");

            stations.MapGet("", async (IStationService stationService, CancellationToken cancellationToken) =>
            {
                var response = await stationService.GetStationNamesAsync(cancellationToken);

                return Results.Ok(response);
            });

            stations.MapGet("data", async (IStationService stationService, CancellationToken cancellationToken) =>
            {
                var response = await stationService.GetStationDataAsync(cancellationToken);

                return Results.Ok(response);
            });

            stations.MapGet("coords", async (IStationService stationService, CancellationToken cancellationToken) =>
            {
                var response = await stationService.GetStationCoordsAsync(cancellationToken);

                return Results.Ok(response);
            });

            return routes;
        }
    }
}
