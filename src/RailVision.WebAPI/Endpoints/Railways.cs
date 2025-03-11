using RailVision.Application.Abstractions;

namespace RailVision.WebAPI.Endpoints
{
    public static class Railways
    {
        public static IEndpointRouteBuilder RegisterRailwaysEndpoints(this IEndpointRouteBuilder routes)
        {
            var railways = routes.MapGroup("api/railways");

            railways.MapGet("", async (IRailwayService railwayService, CancellationToken cancellationToken) =>
            {
                var response = await railwayService.GetRailwayDataAsync(cancellationToken);

                return Results.Ok(response);
            });

            railways.MapGet("lines", async (IRailwayService railwayService, CancellationToken cancellationToken) =>
            {
                var response = await railwayService.GetRailwayLinesAsync(cancellationToken);

                return Results.Ok(response);
            });

            return routes;
        }
    }
}
