using RailVision.Application.Abstractions;

namespace RailVision.WebAPI.Endpoints
{
    public static class Railways
    {
        public static IEndpointRouteBuilder RegisterRailwaysEndpoints(this IEndpointRouteBuilder routes)
        {
            var railways = routes.MapGroup("api/railways");

            railways.MapGet("data", async (IRailwayService railwayService, CancellationToken cancellationToken) =>
            {
                var response = await railwayService.GetRailwaysDataAsync(cancellationToken);

                return Results.Ok(response);
            });

            railways.MapGet("", async (IRailwayService railwayService, CancellationToken cancellationToken) =>
            {
                var response = await railwayService.GetAllAsync(cancellationToken);

                return Results.Ok(response);
            }).WithResponseCache();

            railways.MapGet("{id:guid}", async (Guid id, IRailwayService railwayService, CancellationToken cancellationToken) =>
            {
                var response = await railwayService.GetByIdAsync(id, cancellationToken);

                return Results.Ok(response);
            });

            railways.MapGet("{id:long}", async (long id, IRailwayService railwayService, CancellationToken cancellationToken) =>
            {
                var response = await railwayService.GetByIdAsync(id, cancellationToken);

                return Results.Ok(response);
            });

            return routes;
        }
    }
}
